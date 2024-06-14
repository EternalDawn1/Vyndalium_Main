using Editor;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Reflection;
using GeneralGame.HUD;

namespace GeneralGame
{
	public class HierarchyEditor : GraphicsView
	{
	    private List<Component> _components;

	    public HierarchyEditor(Widget parent) : base(parent)
	    {
	        _components = new List<Component>();

	        // Layout
	        Layout = Layout.Column();
	        Layout.Margin = 10;

	        // Add components to the list
	        foreach (var component in _components)
	        {
	            var componentName = Layout.Add(new StringProperty(this)
	            {
	                Value = component.GetType().Name
	            }, 0);

	            // Display methods of the component
	            foreach (var method in component.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance))
	            {
	                Layout.Add(new StringProperty(this)
	                {
	                    Value = method.Name
	                }, 0);
	            }

	            Layout.AddSpacingCell(4);
	        }
	    }
	}
}