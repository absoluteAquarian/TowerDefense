using AbsoluteCommons.Collections;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace AbsoluteCommons.Components {
	[AddComponentMenu("Absolute Commons/Time/Timer Tracker")]
	public class TimersTracker : MonoBehaviour {
		private readonly FreeList<Timer> _timers = new();
		private readonly SparseSet _indices = new(16);
		private readonly Dictionary<long, int> _knownTimers = new();

		private readonly Queue<Timer> _pendingAdditions = new();
		private readonly Queue<Timer> _pendingRemovals = new();

		private static int _instanceCount;

		public void AddTimer(Timer timer) {
			if (timer.isTracked)
				throw new ArgumentException("Timer has already been added to a tracker");
			
			timer.isTracked = true;
			timer.uniqueID = _instanceCount++;
			_pendingAdditions.Enqueue(timer);
		}

		private void ActuallyAddTimer(Timer timer) {
			int index = _timers.Insert(timer);
			_indices.Add(index);
			_knownTimers.Add(timer.uniqueID, index);

			if (!timer.repeating)
				new DestroyTimerOnCompletion(this, timer).Track();
		}

		public void RemoveTimer(Timer timer) {
			if (!timer.isTracked)
				throw new ArgumentException("Timer has not been added to a tracker");

			if (!_knownTimers.ContainsKey(timer.uniqueID))
				throw new ArgumentException($"Timer {timer.uniqueID} was not found in this tracker");
			
			timer.isTracked = false;
			_pendingRemovals.Enqueue(timer);
		}

		private void ActuallyRemoveTimer(Timer timer) {
			int index = _knownTimers[timer.uniqueID];
			_knownTimers.Remove(timer.uniqueID);
			_timers.Remove(index);
			_indices.Remove(index);
		}

		private void Update() {
			foreach (var timer in _timers.Enumerate(_indices))
				timer.Tick();

			// Timer addition/removal needs to be delayed to avoid modifying the list while iterating over it
			while (_pendingAdditions.TryDequeue(out Timer timer))
				ActuallyAddTimer(timer);
			while (_pendingRemovals.TryDequeue(out Timer timer))
				ActuallyRemoveTimer(timer);
		}

		private class DestroyTimerOnCompletion {
			private readonly TimersTracker _tracker;
			private readonly Timer _timer;

			public DestroyTimerOnCompletion(TimersTracker tracker, Timer timer) {
				_tracker = tracker;
				_timer = timer;
			}

			internal void Track() => _timer.OnComplete += Destroy;

			public void Destroy() => _tracker.RemoveTimer(_timer);
		}
	}

	[Serializable, Inspectable]
	public sealed class Timer {
		public enum State {
			NotStarted,
			Running,
			Paused,
			Completed
		}

		public readonly float initial;
		public readonly float target;
		public readonly float step;
		public readonly bool repeating;

		[Header("Private Members")]
		[SerializeField, ReadOnly] private float _current;
		[SerializeField, ReadOnly] private State _state = State.NotStarted;
		[SerializeField, ReadOnly] internal long uniqueID = -1;
		internal bool isTracked;
		
		internal event Action OnComplete;

		public long ID => uniqueID;

		public float Current => _current;

		public State CurrentState => _state;

		public bool IsRepeating => repeating;

		private Timer(float initial, float target, float step, bool repeating, Action onComplete) {
			if (step == 0)
				throw new ArgumentException("Step must be non-zero");
			else if (step > 0 && target < initial)
				throw new ArgumentException("Target must be greater than initial when step is positive");
			else if (step < 0 && target > initial)
				throw new ArgumentException("Target must be less than initial when step is negative");

			this.initial = initial;
			this.target = target;
			this.step = step;
			this.repeating = repeating;
			_current = initial;
			OnComplete += onComplete;
		}

		public static Timer CreateCountup(Action onCompleted, float target, float step, bool repeating = false) {
			return new Timer(0, target, step, repeating, onCompleted);
		}

		public static Timer CreateCountup(Action onCompleted, float initial, float target, float step, bool repeating = false) {
			return new Timer(initial, target, step, repeating, onCompleted);
		}

		public static Timer CreateCountdown(Action onCompleted, float target, float step, bool repeating = false) {
			return new Timer(target, 0, step, repeating, onCompleted);
		}

		public static Timer CreateCountdown(Action onCompleted, float initial, float target, float step, bool repeating = false) {
			return new Timer(initial, target, step, repeating, onCompleted);
		}

		public void Start() {
			if (!repeating && _state == State.Completed)
				throw new InvalidOperationException("A non-repeating timer cannot be started after it has finished");

			_state = State.Running;
			_current = initial;
		}

		public void Pause() {
			if (_state != State.Running && _state != State.Paused)
				throw new InvalidOperationException("Cannot pause a timer that is not running");

			_state = State.Paused;
		}

		public void Stop() {
			if (!repeating && _state == State.Completed)
				throw new InvalidOperationException("A non-repeating timer cannot be stopped after it has finished");

			_state = State.NotStarted;
			_current = initial;
		}

		private bool HasTargetBeenReached() {
			// Equals case is separated since it will be encountered more often
			return _current == target || (step > 0 && _current > target) || (step < 0 && _current < target);
		}

		internal void Tick() {
			if (_state != State.Running)
				return;

			_current += Time.deltaTime;

			if (repeating) {
				while (HasTargetBeenReached()) {
					// Allow any excess time to be carried over to the next iteration
					_current -= target - initial;
					OnComplete?.Invoke();
				}
			} else if (HasTargetBeenReached()) {
				// The non-repeating timer has finished
				_current = target;
				_state = State.Completed;
				OnComplete?.Invoke();
			}
		}
	}
}
