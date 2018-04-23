using JustABackup.Database.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JustABackup.Database.Entities
{
    public class ProviderPropertyAttribute : IEquatable<ProviderPropertyAttribute>
    {
        [Key]
        [Required]
        public int ID { get; set; }

        [Required]
        public PropertyAttribute Name { get; set; }

        public string Value { get; set; }

        public ProviderPropertyAttribute()
        {
        }

        public ProviderPropertyAttribute(PropertyAttribute name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        #region IEquatable<ProviderProperty>
        public bool Equals(ProviderPropertyAttribute other)
        {
            return Equals(other, false);
        }
        
        public bool Equals(ProviderPropertyAttribute other, bool includeId)
        {
            if (other == null)
                return false;

            if (includeId && ID != other.ID)
                return false;

            if (Name != other.Name)
                return false;

            if (Value != other.Value)
                return false;

            return true;
        }
        #endregion

        #region Overrides
        public override bool Equals(object obj)
        {
            return Equals(obj as ProviderPropertyAttribute);
        }

        public override int GetHashCode()
        {
            var hashCode = -1338312766;
            hashCode = hashCode * -1521134295 + Name.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Value);
            return hashCode;
        }
        #endregion
    }
}
