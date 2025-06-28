// Author  : Alexander Nava 
// Contact : Alexander.Nava.Contact@Gmail.com
// License : For personal use excluding any artificial or machine learning this is licensed under MIT license.
// License : For commercial software(excluding derivative work to make libraries with the same functionality in any language) use excluding any artificial or machine learning this is licensed under MIT license.
// License : If you are a developer making money writing this software it is expected for you to donate, and thus will be given to you for any prepose other than use with Artificial Intelligence or Machine Learning this is licensed under MIT license.
// License : To any Artificial Intelligence or Machine Learning use there is no license given and is forbidden to use this for learning preposes or for anyone requesting you use these libraries, if done so will break the terms of service for this code and you will be held liable.
// License : For libraries or derivative works that are created based on the logic, patterns, or functionality of this library must inherit all licenses here in.
// License : If you are not sure your use case falls under any of these clauses please contact me through the email above for a license.

namespace Levels.Unity {
	// USINGS -- START
	#region USINGS
	using System;
	using System.Collections.Generic;

	using UnityEngine;

	using Sirenix.OdinInspector;

	using Levels.Core.DI;
	using Levels.Core;
	using Levels.Core.Behaviours;
	#endregion
	// USINGS -- END

	// TODO : What use cases do we need a Modifier for this? Or more is there a use case where we wouldn't want it?
	// TODO : Do we need a ReadOnly value?
	public interface IShare<T> :
		IPipe<T>,
		IGive<ReadOnly<T>>,
		INotify<ReadOnly<T>>,
		IAttribute<Share.Attributes>,
		IRegister {
	}

	/// <summary>
	/// Can be used to not have to have a more specific default register implementation.
	/// </summary>
	/// <typeparam name="TDefinition">The inheriting class.</typeparam>
	/// <typeparam name="T">The type being shared.</typeparam>
	public abstract class _Share_<TInherited, T> : _Share_<T>
		where TInherited : _Share_<TInherited, T> {

		// IRegister -- START
		#region IRegister
		public override void Register(in Scope.ID scope, object source)
		{
			base.Register(scope, source);

			scope.RegisterService(this as TInherited, Label, this);
			scope.RegisterService<_Share_<T>>(this, Label, this);
		}

		public override void Unregister()
		{
			_scope.UnregisterAll<TInherited>(this);
			_scope.UnregisterAll<_Share_<T>>(this);
		}
		#endregion
		// IRegister -- END
	}

	// TODO : Split out to a base class Data.
	// TODO : Move IModifiable<T> to the scope section. So anyone can register a modifier 
	// To the data label from the scope?
	public abstract class _Share_<T> : MonoBehaviour,
		IShare<T> {

		// Don't ask about the naming here.
		[SerializeField, BoxGroup("SETTINGS")]
		protected T _value;
		T IData<T>._Value { get => this.Get(); set => _value = value; }

		// IAttribute -- START
		#region IAttribute
		[field: SerializeField, BoxGroup("SETTINGS")]
		public string Label { get; set; }
		[field: SerializeField, BoxGroup("SETTINGS")]
		public Share.Attributes Attribute { get; set; }
		#endregion
		// IAttribute -- END

		// Initialize -- START
		#region Initialize
		protected Scope.ID _scope;
		public virtual void Register(in Scope.ID scope, object source) {
			this._scope = scope;
			Exports<IShare<T>>.Register(scope, Entry.Factory.Scoped<IShare<T>>(() => this, Label, this));
		}
		public virtual void Unregister() {
			Exports<IShare<T>>.UnregisterAll(_scope, this);
		}
		#endregion
		// Initialize -- END

		// IModify -- START
		#region IModify
		protected readonly Dictionary<object, List<IConfig<T>>> modifiers = new();
		Dictionary<object, List<IConfig<T>>> IModify<T>._Modifiers => modifiers;

		bool IModify<T>._DirtyModifiers { get; set; }
		private T _valueModified;
		ref T IModify<T>._ValueModified { get => ref _valueModified; }
		#endregion
		// IModify -- END

		// OUT -- START
		#region OUT
		[field: SerializeField, BoxGroup("OUT")]
		public Action<T> OnValueSet { get; set; }
		[field: SerializeField, BoxGroup("OUT")]
		public Func<T, T> OnValueModified { get; set; }

		[field: SerializeField, BoxGroup("OUT")]
		_Action_<T> INotify<T>.Listeners { get; set; } = new();
		#endregion
		// OUT -- END

		// ReadOnly -- START
		#region ReadOnly
		[field: SerializeField, BoxGroup("OUT")]
		_Action_<ReadOnly<T>> INotify<ReadOnly<T>>.Listeners { get; set; } = new();
		public ReadOnly<T> Give() => new ReadOnly<T>(this.Get());
		#endregion
		// ReadOnly -- END
	}

	public class Share<T> : IShare<T> {
		protected T _value;
		T IData<T>._Value { get => _value; set => _value = value; }

		// IAttribute -- START
		#region IAttribute
		public string Label { get; set; }
		public Share.Attributes Attribute { get; set; }
		#endregion
		// IAttribute -- END

		// Initialize -- START
		#region Initialize
		public Share(T value, string label, Share.Attributes attributes) {
			_value = value;
			Label = label;
			Attribute = attributes;
		}
		
		public ReadOnly<T> Give() => new ReadOnly<T>(this.Get());

		protected Scope.ID _scope;
		public virtual void Register(in Scope.ID scope, object source) {
			this._scope = scope;
			Exports<IShare<T>>.Register(scope, Entry.Factory.Scoped<IShare<T>>(() => this, Label, this));
		}
		public virtual void Unregister() {
			Exports<IShare<T>>.UnregisterAll(_scope, this);
		}
		#endregion
		// Initialize -- END

		// IModify -- START
		#region IModify
		protected readonly Dictionary<object, List<IConfig<T>>> modifiers = new();
		Dictionary<object, List<IConfig<T>>> IModify<T>._Modifiers => modifiers;
		bool IModify<T>._DirtyModifiers { get; set; }
		private T _valueModified;
		ref T IModify<T>._ValueModified { get => ref _valueModified; }
		#endregion
		// IModify -- END

		// OUT -- START
		#region OUT
		public Action<T> OnValueSet { get; set; }
		public Func<T, T> OnValueModified { get; set; }
		_Action_<T> INotify<T>.Listeners { get; set; }
		_Action_<ReadOnly<T>> INotify<ReadOnly<T>>.Listeners { get; set; }
		#endregion
		// OUT -- END
	}

	public abstract class Share<TInherited, T> : Share<T>
		where TInherited : Share<TInherited, T> {

		// Initialize -- START
		#region Initalize
		protected Share(T value, string label, Share.Attributes attributes) : base(value, label, attributes) {
		}

		public override void Register(in Scope.ID scope, object source) {
			base.Register(scope, source);
			
			scope.RegisterService(this as TInherited, Label, this);
			scope.RegisterService<Share<T>>(this, Label, this);
		}

		public override void Unregister() {
			_scope.UnregisterAll<TInherited>(this);
			_scope.UnregisterAll<Share<T>>(this);
		}
		#endregion
		// Initialize -- END

	}

	public static class _Share_Extends {
		public static INotify<T> Interface_INotify<T>(this IShare<T> source)
			=> source;
		public static INotify<ReadOnly<T>> Interface_INotifyRO<T>(this IShare<T> source)
			=> source;

		public static _Action_<T> INotify_Listeners<T>(this IShare<T> source)
			=> source.Interface_INotify<T>().Listeners;
		public static _Action_<ReadOnly<T>> INotify_ROListeners<T>(this IShare<T> source)
			=> source.Interface_INotifyRO<T>().Listeners;
	}

	// TODO : Move to its own file
	public readonly struct ReadOnly<T> : IGet<T> {
		private readonly T _value;
		T IData<T>._Value {
			get => _value;
			set { }
		}

		public ReadOnly(T value) {
			this._value = value;
		}
		
		public T Get() {
			return _value;
		}
	}

	public partial class Share
	{
		[System.Flags]
		public enum Attributes {
			None = 0,

			Sink1 = 1 << 0,
			Sink2 = 1 << 1,
			Sink3 = 1 << 2,
			Sink4 = 1 << 3,
			Sink5 = 1 << 4,
			Sink6 = 1 << 5,
			Sink7 = 1 << 6,
			Sink8 = 1 << 7,
			Sink9 = 1 << 8,
			Sink10 = 1 << 9,

			Local = 1 << 10,
			Network = 1 << 11,

			AllSinks = Sink1 | Sink2 | Sink3 | Sink4 | Sink5 | Sink6 | Sink7 | Sink8 | Sink9 | Sink10,
			AllDestinations = Local | Network
		}
	}
}
