using JustABackup.Database.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace JustABackup.Database.Entities
{
    public class ProviderProperty : IEquatable<ProviderProperty>
    {
        [Key]
        [Required]
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string TypeName { get; set; }

        public string Description { get; set; }

        [Required]
        public PropertyType Type { get; set; }

        public List<ProviderPropertyAttribute> Attributes { get; set; }

        public ProviderProperty()
        {
            Attributes = new List<ProviderPropertyAttribute>();
        }

        #region Overrides
        public override bool Equals(object? obj)
        {
            if(obj is ProviderProperty pp)
                return Equals(pp);

            return false;
        }

        public bool Equals(ProviderProperty? other)
        {
            return Equals(other, true);
        }

        public bool Equals(ProviderProperty? other, bool includeId)
        {
            if (other == null)
                return false;

            if (includeId && ID != other.ID)
                return false;

            if (Name != other.Name)
                return false;

            if (TypeName != other.TypeName)
                return false;

            if (Type != other.Type)
                return false;

            var firstNotSecond = Attributes.Except(other.Attributes).ToList();
            var secondNotFirst = other.Attributes.Except(Attributes).ToList();
            if (firstNotSecond.Any() || secondNotFirst.Any())
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, Name, TypeName, Type);
        }
        #endregion
    }
}
