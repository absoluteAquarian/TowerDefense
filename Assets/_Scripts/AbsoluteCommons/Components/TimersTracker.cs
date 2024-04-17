using AbsoluteCommons.Attributes;
using AbsoluteCommons.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
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

		// Editor stuff
		[SerializeField, ReadOnly] private Timer[] _timersList = Array.Empty<Timer>();

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
				timer.OnComplete += RemoveTimer;
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

			// If the editor is open, update the list for inspection
			if (Application.isEditor)
				_timersList = _timers.Enumerate(_indices).ToArray();
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
		public readonly bool repeating;

		[Header("Private Members")]
		[SerializeField, ReadOnly] private float _current;
		[SerializeField, ReadOnly] private State _state = State.NotStarted;
		[SerializeField, ReadOnly] internal long uniqueID = -1;
		internal bool isTracked;
		private readonly bool decrement;
		
		internal event Action<Timer> OnComplete;

		public long ID => uniqueID;

		public float Current => _current;

		public State CurrentState => _state;

		public bool IsRepeating => repeating;

		private Timer(float initial, float target, bool repeating, Action<Timer> onComplete) {
			if (initial == target)
				throw new ArgumentException("Initial and target values cannot be equal");

			this.initial = initial;
			this.target = target;
			this.repeating = repeating;
			_current = initial;
			decrement = initial > target;
			OnComplete += onComplete;
		}

		public static Timer CreateCountup(Action<Timer> onCompleted, float target, bool repeating = false) {
			return new Timer(0, target, repeating, onCompleted);
		}

		public static Timer CreateCountup(Action<Timer> onCompleted, float initial, float target, bool repeating = false) {
			return new Timer(initial, target, repeating, onCompleted);
		}

		public static Timer CreateCountdown(Action<Timer> onCompleted, float initial, bool repeating = false) {
			return new Timer(initial, 0, repeating, onCompleted);
		}

		public static Timer CreateCountdown(Action<Timer> onCompleted, float initial, float target, bool repeating = false) {
			return new Timer(initial, target, repeating, onCompleted);
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
			return _current == target || (!decrement && _current > target) || (decrement && _current < target);
		}

		internal void Tick() {
			if (_state != State.Running)
				return;

			_current = decrement ? _current - Time.deltaTime : _current + Time.deltaTime;

			if (repeating) {
				while (HasTargetBeenReached()) {
					// Allow any excess time to be carried over to the next iteration
					_current -= target - initial;
					OnComplete?.Invoke(this);
				}
			} else if (HasTargetBeenReached()) {
				// The non-repeating timer has finished
				_current = target;
				_state = State.Completed;
				OnComplete?.Invoke(this);
			}
		}
	}
}
