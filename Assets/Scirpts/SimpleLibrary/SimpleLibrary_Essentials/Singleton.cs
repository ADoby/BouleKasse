/*Author: Tobias Zimmerlin
 * 30.01.2015
 * V1
 *
 */

using System;
using System.Collections;
using UnityEngine;

namespace SimpleLibrary
{
	//Monobehaviour Singleton for Unity use
	public class Singleton<ChildType> : MonoBehaviour where ChildType : MonoBehaviour
	{
		//Never use this directly, thats why its private NOT protected
		private static ChildType instance = null;

		//Use this to get the current single instance of this type
		public static ChildType Instance
		{
			get
			{
				if (instance == null)
					instance = FindObjectOfType<ChildType>();
				return (ChildType)instance;
			}
			set
			{
				instance = value;
			}
		}

		protected virtual void Awake()
		{
			Instance = this as ChildType;
		}
	}
}