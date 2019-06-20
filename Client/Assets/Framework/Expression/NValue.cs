// /**************************************************************************************************
//  * Copyright (C) 2017-2019 FengYunChuanShuo
//  * 
//  * 
//  * 
//  *Author:     Noe  <chglove@live.cn>
//  *Version:    0.1
//  *Created:    2018-12-24      14:07
//  *LastModify：2018-12-24      14:17
//  ***************************************************************************************************/

#region

using System;

#endregion

namespace NoeExpression
{
    using baseValue = Double;
    public class NValueConst : NValue
    {
        public NValueConst(object rValue)
            : base(rValue)
        {
        }

        public override object Value
        {
            get { return Object; }
            set
            {
                throw new Exception("不能对const的值进行赋值：" + Object);
            }
        }
    }


    public class NValue
    {
        public static NValue False = new NValueConst(false);
        public static NValue True = new NValueConst(true);
        public static NValue Zero = new NValueConst(0);
        public static NValue One = new NValueConst(1);
        protected object Object;
        protected Type Type;

        public NValue()
        {
            Set(0);
        }

        public NValue(object rValue)
        {
            Set(rValue);
        }

        public virtual object Value
        {
            get { return Object; }
            set { Set(value); }
        }

        public NValue Clone()
        {
            var rValue = new NValue();
            rValue.Type = Type;
            rValue.Object = Object;

            return rValue;
        }

        public object To(Type rType)
        {
            return Convert.ChangeType(Object, rType);
        }

        public T To<T>()
        {
            if (Type == typeof (T))
                return (T) Object;
            return (T) Convert.ChangeType(Object, typeof (T));
        }

        public void Set(object value)
        {
            baseValue nSingleValue;

            var rValueType = value.GetType();

            if (rValueType == typeof (baseValue))
            {
                Object = value;
            }
            else if (rValueType == typeof (bool)) // 布尔数值
            {
                if ((bool) value)
                {
                    Object = (baseValue) 1;
                }
                else
                {
                    Object = (baseValue) 0;
                }
            }
            else if (rValueType == typeof (int))
            {
                Object = (baseValue) (int) value;
            }
            // 文字变量
            else if (rValueType == typeof (string))
            {
                Object = value;
            }
            else if (baseValue.TryParse(value.ToString(), out nSingleValue)) // 尝试进行转换
            {
                Object = nSingleValue;
            }
            // 其余不能转换的对象
            else
            {
                Object = Convert.ChangeType(value, typeof (baseValue));
            }

            Type = Object.GetType();
        }


        public void SetValueString(string strValue)
        {
            Object = strValue;
            Type = Object.GetType();
        }

        public string GetStringValue()
        {
            if (Type == typeof (string))
                return (string) Object;
            return Object.ToString();
        }

        #region

        protected bool Equals(NValue other)
        {
            return Equals(Object, other.Object) && Equals(Type, other.Type);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NValue)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Object?.GetHashCode() ?? 0) * 397) ^ (Type?.GetHashCode() ?? 0);
            }
        }

        #endregion

        // 重载操作符
        public static bool operator ==(NValue rValue1, NValue rValue2)
        {
            if (rValue1.Type == rValue2.Type)
            {
                if (rValue1.Object.Equals(rValue2.Object))
                {
                    return true;
                }
            }

            return false;
        }

        // 重载操作符
        public static bool operator !=(NValue rValue1, NValue rValue2)
        {
            if (rValue1.Type == rValue2.Type)
            {
                return !rValue1.Object.Equals(rValue2.Object);
            }

            return false;
        }

        // 重载操作符
        public static bool operator >(NValue rValue1, NValue rValue2)
        {
            if (rValue1.Type == rValue2.Type)
            {
                if (rValue1.Type == typeof (baseValue))
                {
                    return (baseValue) rValue1.Object > (baseValue) rValue2.Object;
                }
            }

            return false;
        }

        public static bool operator <(NValue rValue1, NValue rValue2)
        {
            if (rValue1.Type == rValue2.Type)
            {
                if (rValue1.Type == typeof (baseValue))
                {
                    return (baseValue) rValue1.Object < (baseValue) rValue2.Object;
                }
            }

            return false;
        }

        // 重载操作符
        public static bool operator >=(NValue rValue1, NValue rValue2)
        {
            if (rValue1.Type == rValue2.Type)
            {
                if (rValue1.Type == typeof (baseValue))
                {
                    return (baseValue) rValue1.Object >= (baseValue) rValue2.Object;
                }
            }


            return false;
        }

        public static bool operator <=(NValue rValue1, NValue rValue2)
        {
            if (rValue1.Type == rValue2.Type)
            {
                if (rValue1.Type == typeof (baseValue))
                {
                    return (baseValue) rValue1.Object <= (baseValue) rValue2.Object;
                }
            }


            return false;
        }

        public static NValue operator *(NValue rValue1, NValue rValue2)
        {
            if (rValue1.Type == rValue2.Type)
            {
                if (rValue1.Type == typeof (baseValue))
                {
                    return new NValue((baseValue) rValue1.Object*(baseValue) rValue2.Object);
                }
            }

            return Zero;
        }

        public static NValue operator /(NValue rValue1, NValue rValue2)
        {
            if (rValue1.Type == rValue2.Type)
            {
                if (rValue1.Type == typeof (baseValue))
                {
                    return new NValue((baseValue) rValue1.Object/(baseValue) rValue2.Object);
                }
            }

            return Zero;
        }

        public static NValue operator +(NValue rValue1, NValue rValue2)
        {
            if (rValue1.Type == rValue2.Type)
            {
                if (rValue1.Type == typeof (baseValue))
                {
                    return new NValue((baseValue) rValue1.Object + (baseValue) rValue2.Object);
                }
            }

            return Zero;
        }

        public static NValue operator -(NValue rValue1, NValue rValue2)
        {
            if (rValue1.Type == rValue2.Type)
            {
                if (rValue1.Type == typeof (baseValue))
                {
                    return new NValue((baseValue) rValue1.Object - (baseValue) rValue2.Object);
                }
            }

            return Zero;
        }

        public static NValue operator %(NValue rValue1, NValue rValue2)
        {
            if (rValue1.Type == rValue2.Type)
            {
                if (rValue1.Type == typeof (baseValue))
                {
                    return new NValue((baseValue) rValue1.Object%(baseValue) rValue2.Object);
                }
            }

            return Zero;
        }

        public static implicit operator NValue(int rValue)
        {
            return new NValue(rValue);
        }

        public static implicit operator NValue(double rValue)
        {
            return new NValue(rValue);
        }

        public static implicit operator NValue(uint rValue)
        {
            return new NValue(rValue);
        }

        public static implicit operator NValue(long rValue)
        {
            return new NValue(rValue);
        }

        public static implicit operator NValue(ulong rValue)
        {
            return new NValue(rValue);
        }

        public static implicit operator NValue(string rValue)
        {
            return new NValue(rValue);
        }
    }
}