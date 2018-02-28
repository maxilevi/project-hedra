/*
 * Author: Zaphyk
 * Date: 03/02/2016
 * Time: 11:58 p.m.
 *
 */
using System;

namespace Hedra.Engine.Management
{
	public interface ICullable
	{
		bool Rendered {get; set;}
		bool Enabled {get; set;}
		OpenTK.Vector3 Position {get; set;}
		OpenTK.Vector3 Size{get; set;}
		RenderShape Shape {get; set;}
		bool DontCull {get; set;}
		
	}
}

public enum RenderShape{
	CUBE,
	POINT,
	SPHERE
}