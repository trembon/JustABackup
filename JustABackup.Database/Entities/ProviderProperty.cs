using JustABackup.Database.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public override bool Equals(object obj)
        {
            return Equals(obj as ProviderProperty);
        }

        public bool Equals(ProviderProperty other)
        {
            return Equals(other, true);
        }

        public bool Equals(ProviderProperty other, bool includeId)
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

            // TODO: compare attributes

            return true;
        }

        public override int GetHashCode()
        {
            var hashCode = -1338312766;
            hashCode = hashCode * -1521134295 + ID.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TypeName);
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            return hashCode;
        }
        #endregion
    }
}
