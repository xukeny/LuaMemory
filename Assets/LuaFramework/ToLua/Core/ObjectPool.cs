/*
Copyright (c) 2015-2016 topameng(topameng@qq.com)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace LuaInterface
{
    public class LuaObjectPool
    {        
#if UNITY_EDITOR
        class PoolNode
        {
            public int index;
            public object obj;
            public string debugLog = "null";

            public PoolNode(int index, object obj)
            {
                this.index = index;
                this.obj = obj;
                debugLog = getPath(obj);
            }

            public static string getPath(object obj)
            {
                UnityEngine.Transform child = null;
                if (obj == null)
                    return "null";
                else if (obj is UnityEngine.Component)
                    child = (obj as UnityEngine.Component).transform;
                else if (obj is UnityEngine.GameObject)
                    child = (obj as UnityEngine.GameObject).transform;
                else if (obj is UnityEngine.Object)
                    return obj.GetType().Name + "," + (obj as UnityEngine.Object).name;
                else
                    return obj.GetType().Name;

                List<string> strlist = new List<string>();
                while ((child != null))
                {
                    strlist.Add(child.name);
                    child = child.parent;
                }
                strlist.Reverse();
                string path = obj.GetType().Name + ":" + string.Join("/", strlist.ToArray());
                return path;
            }
        }

        public string GetPoolListStr(ObjectTranslator _translator)
        {
            StringBuilder sb = new StringBuilder("LuaPool:\n");
            for (int i = 0; i < list.Count; i++)
            {
                sb.Append(i + ":" + (isNull(list[i].obj) ? "null," : "actt,") + list[i].debugLog + "\n");
                //if (isNull(list[i].obj))
                //{
                //    _translator.RemoveObject(i);
                //}
            }
            return sb.ToString();
        }

        private bool isNull(object obj)
        {
            if (obj == null) return true;
            return obj.Equals(null);
        }
#else
        class PoolNode
        {
            public int index;
            public object obj;

            public PoolNode(int index, object obj)
            {
                this.index = index;
                this.obj = obj;
            }
        }

#endif
        public void ClearLuaRef(ObjectTranslator _translator)
        {
#if UNITY_EDITOR
            StringBuilder sb = new StringBuilder("ClearLuaRef:\n");
#endif
            for (int i = 0; i < count; i++)
            {
                if (list[i].obj is UnityEngine.Object)
                {
                    if (list[i].obj.Equals(null))
                    {
#if UNITY_EDITOR
                        sb.Append(i + ":dispose," + list[i].debugLog + "\n");
#endif
                        _translator.Destroy(i);
                    }
                }
            }
#if UNITY_EDITOR
            Debugger.Log(sb.ToString());
#endif
        }

        private List<PoolNode> list;
        //同lua_ref策略，0作为一个回收链表头，不使用这个位置
        private PoolNode head = null;   
        private int count = 0;

        public LuaObjectPool()
        {
            list = new List<PoolNode>(1024);
            head = new PoolNode(0, null);
            list.Add(head);
            list.Add(new PoolNode(1, null));
            count = list.Count;
        }

        public object this[int i]
        {
            get 
            {
                if (i > 0 && i < count)
                {
                    return list[i].obj;
                }

                return null;
            }
        }

        public void Clear()
        {
            list.Clear();
            head = null;
            count = 0;
        }

        public int Add(object obj)
        {
            int pos = -1;

            if (head.index != 0)
            {
                pos = head.index;
                list[pos].obj = obj;
#if UNITY_EDITOR
                list[pos].debugLog = PoolNode.getPath(obj);
#endif
                head.index = list[pos].index;
            }
            else
            {
                pos = list.Count;
                list.Add(new PoolNode(pos, obj));
                count = pos + 1;
            }

            return pos;
        }

        public object TryGetValue(int index)
        {
            if (index > 0 && index < count)
            {
                return list[index].obj;                
            }
            
            return false;
        }

        public object Remove(int pos)
        {            
            if (pos > 0 && pos < count)
            {
                object o = list[pos].obj;
                list[pos].obj = null;
#if UNITY_EDITOR
                list[pos].debugLog = "Remove";
#endif
                list[pos].index = head.index;
                head.index = pos;

                return o;
            }

            return null;
        }

        public object Destroy(int pos)
        {
            if (pos > 0 && pos < count)
            {
                object o = list[pos].obj;
                list[pos].obj = null;
#if UNITY_EDITOR
                list[pos].debugLog = "Destroy:" + list[pos].debugLog;
#endif
                return o;
            }

            return null;
        }

        public object Replace(int pos, object o)
        {
            if (pos > 0 && pos < count)
            {
                object obj = list[pos].obj;
                list[pos].obj = o;
#if UNITY_EDITOR
                list[pos].debugLog = PoolNode.getPath(o);
#endif
                return obj;
            }

            return null;
        }
    }
}