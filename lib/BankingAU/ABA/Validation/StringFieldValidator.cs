﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Banking.AU.ABA.Validation
{
    public class StringFieldValidator<T> : IValidator<T>
    {
        private GetValue<T, string> _get;
        private SetValue<T, string> _set;
        private int _maxLength;
        private RegexValidator<T> _regex;
        public bool AllowNull { get; set; }
        public StringFieldValidator(int maxLength, GetValue<T, string> get)
        {
            AllowNull = false;
            _maxLength = maxLength;
            _get = get;
        }
        public StringFieldValidator(int maxLength, GetValue<T, string> get, SetValue<T, string> set)
            : this(maxLength, get)
        {
            _set = set;
        }
        public StringFieldValidator(int maxLength, Regex charset, GetValue<T, string> get)
            : this(maxLength, get)
        {
            _regex = new RegexValidator<T>(charset, get);
        }
        public StringFieldValidator(int maxLength, Regex charset, GetValue<T, string> get, SetValue<T, string> set)
            : this(maxLength, get)
        {
            _regex = new RegexValidator<T>(charset, get, set);
            _set = set;
        }

        public void Clean(T item)
        {
            var value = _get(item);
            if (!String.IsNullOrEmpty(value))
            {
                _regex.Clean(item);
                value = _get(item);
                if (!String.IsNullOrEmpty(value))
                    _set(item, value.Substring(0, _maxLength));
            }
        }

        public IEnumerable<IError> Validate(T item)
        {
            var value = _get(item);
            if (String.IsNullOrEmpty(value))
            {
                if (!AllowNull)
                    yield return new Error("String field must have a value");
            }
            else
            {
                if (value.Length > _maxLength)
                    yield return new Error(String.Format("'{0}' exceeds max length of {1}", value, _maxLength));
                if (_regex != null)
                    foreach (var e in _regex.Validate(item))
                        yield return e;
            }
        }
    }
}