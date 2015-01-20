// Copyright (c) 2002-2003, Sony Computer Entertainment America
// Copyright (c) 2002-2003, Craig Reynolds <craig_reynolds@playstation.sony.com>
// Copyright (C) 2007 Bjoern Graf <bjoern.graf@gmx.net>
// All rights reserved.
//
// This software is licensed as described in the file license.txt, which
// you should have received as part of this distribution. The terms
// are also available at http://www.codeplex.com/SharpSteer/Project/License.aspx.

using System;
using System.Collections.Generic;
using CocosSharp;
using CocosSharpSteer;

namespace SteeringDemo
{
	public abstract class PlugIn : IPlugIn
	{
        
        public abstract void Open();
		public abstract void Update(float currentTime, float elapsedTime);
		public abstract void Redraw(float currentTime, float elapsedTime);
		public abstract void Close();
		public abstract String Name { get; }
		public abstract IEnumerable<IVehicle> Vehicles { get; }

		// prototypes for function pointers used with PlugIns
		public delegate void PlugInCallBackFunction(PlugIn clientObject);
		public delegate void VoidCallBackFunction();
		public delegate void TimestepCallBackFunction(float currentTime, float elapsedTime);

	    protected readonly IAnnotationService Annotations;
        
        public CCNode CocosSharpNode { get; set; }

		// constructor
	    protected PlugIn(CCNode node, IAnnotationService annotations)
	    {
            CocosSharpNode = node;
	        Annotations = annotations;

			// save this new instance in the registry
			AddToRegistry();
		}

		// default reset method is to do a close then an open
		public virtual void Reset()
		{
			Close();
			Open();
		}

		// default sort key (after the "built ins")
		public virtual float SelectionOrderSortKey { get { return 1.0f; } }

		// default is to NOT request to be initially selected
		public virtual bool RequestInitialSelection { get { return false; } }

		// default function key handler: ignore all
		public virtual void HandleKeys(CCKeys key) { }

		// default "mini help": print nothing
		public virtual void PrintMiniHelpForFunctionKeys() { }

		// returns pointer to the next PlugIn in "selection order"
		public PlugIn Next()
		{
			for (int i = 0; i < _itemsInRegistry; i++)
			{
				if (this == _registry[i])
				{
					bool atEnd = (i == (_itemsInRegistry - 1));
					return _registry[atEnd ? 0 : i + 1];
				}
			}
			return null;
		}

		// format instance to characters for printing to stream
		public override string ToString()
		{
			return String.Format("<PlugIn \"{0}\">", Name);
		}

		// CLASS FUNCTIONS

		// search the class registry for a Plugin with the given name
		public static IPlugIn FindByName(String name)
		{
			if (String.IsNullOrEmpty(name) == false)
			{
				for (int i = 0; i < _itemsInRegistry; i++)
				{
					PlugIn pi = _registry[i];
					String s = pi.Name;
					if (String.IsNullOrEmpty(s) && name == s)
						return pi;
				}
			}
			return null;
		}

		// apply a given function to all PlugIns in the class registry
		public static void ApplyToAll(PlugInCallBackFunction f)
		{
			for (int i = 0; i < _itemsInRegistry; i++)
			{
				f(_registry[i]);
			}
		}

		// sort PlugIn registry by "selection order"
		public static void SortBySelectionOrder()
		{
			// I know, I know, just what the world needs:
			// another inline shell sort implementation...

			// starting at each of the first n-1 elements of the array
			for (int i = 0; i < _itemsInRegistry - 1; i++)
			{
				// scan over subsequent pairs, swapping if larger value is first
				for (int j = i + 1; j < _itemsInRegistry; j++)
				{
					float iKey = _registry[i].SelectionOrderSortKey;
					float jKey = _registry[j].SelectionOrderSortKey;

					if (iKey > jKey)
					{
						PlugIn temporary = _registry[i];
						_registry[i] = _registry[j];
						_registry[j] = temporary;
					}
				}
			}
		}

		// returns pointer to default PlugIn (currently, first in registry)
		public static PlugIn FindDefault()
		{
			// return NULL if no PlugIns exist
			if (_itemsInRegistry == 0) return null;

			// otherwise, return the first PlugIn that requests initial selection
			for (int i = 0; i < _itemsInRegistry; i++)
			{
				if (_registry[i].RequestInitialSelection) return _registry[i];
			}

			// otherwise, return the "first" PlugIn (in "selection order")
			return _registry[0];
		}

		// save this instance in the class's registry of instances
		void AddToRegistry()
		{
			// save this instance in the registry
			_registry[_itemsInRegistry++] = this;
		}

		// This array stores a list of all PlugIns.  It is manipulated by the
		// constructor and destructor, and used in findByName and applyToAll.
		const int TOTAL_SIZE_OF_REGISTRY = 1000;
		static int _itemsInRegistry = 0;
		static readonly PlugIn[] _registry = new PlugIn[TOTAL_SIZE_OF_REGISTRY];
	}
}
