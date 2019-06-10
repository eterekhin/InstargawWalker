using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

#nullable enable
namespace AuthProject.ValueTypes
{
    public class ValueType<T>
        where T : class
    {
        public string Name { get; set; }

        protected ValueType(string name)
        {
            Name = name;
        }

        public static implicit operator ValueType<T>(string value)
        {
            return (ValueType<T>) Activator.CreateInstance(typeof(T), value);
        }

        public static implicit operator string(ValueType<T> valueType)
        {
            return valueType.Name;
        }


        public static implicit operator T(ValueType<T> t)
        {
            return (T) Activator.CreateInstance(typeof(T), t.Name);
        }
    }

    public abstract class ValueTypeValidationHandler<T>
    {
        protected class ValueTypeError
        {
            public ValueTypeError(Func<T, IEnumerable<ValidationResult>?> validationFunc)
            {
                ValidationFunc = validationFunc;
            }

            protected virtual Func<T, IEnumerable<ValidationResult>?> ValidationFunc { get; }
        }


        protected abstract IReadOnlyCollection<ValueTypeError> Validators { get; }
    }

    public class EfCoreValueType<T> : ValueType<T>
        where T : class
    {
        public EfCoreValueType() : this(string.Empty)
        {
        }

        protected EfCoreValueType(string name) : base(name)
        {
        }
    }

    public class Email : ValueType<Email>
    {
        public Email(string name) : base(name)
        {
            var emailAddressAttribute = new EmailAddressAttribute();
            if (!emailAddressAttribute.IsValid(name))
                throw new ArgumentException(emailAddressAttribute.ErrorMessage);
        }
    }

    public class Password : ValueType<Password>
    {
        public Password(string name) : base(name)
        {
        }

        public HashedPassword Hash()
        {
            // todo реализовать хэширование паролей
            return new HashedPassword(Name);
        }
    }


    public class HashedPassword : ValueType<HashedPassword>
    {
        public string EncodePassword()
        {
            // todo реализовать расхэширование паролей
            throw new NotImplementedException();
        }

        public HashedPassword(string name) : base(name)
        {
        }
    }

    public class AccessToken : EfCoreValueType<AccessToken>
    {
        public AccessToken(string name) : base(name)
        {
            if (name.Split('.').Length != 3) throw new ArgumentException();
            // other validation
        }
    }

    public class RefreshToken : ValueType<RefreshToken>
    {
        public RefreshToken(string name) : base(name)
        {
        }
    }

    public class JwtTokenValidateHandler : ValueTypeValidationHandler<AccessToken>
    {
        readonly Func<AccessToken, IEnumerable<ValidationResult>?> validationFunc = x =>
        {
            string name = x;
            return name.Split('.').Length != 3
                ? new[] {new ValidationResult("JWT TOKEN FORMAT - ####.####.####")}
                : null;
        };

        public JwtTokenValidateHandler()
        {
            Validators = new List<ValueTypeError>
            {
                new ValueTypeError(validationFunc)
            };
        }

        protected override IReadOnlyCollection<ValueTypeError> Validators { get; }
    }
}