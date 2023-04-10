﻿using JustABackup.Database.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace JustABackup.Database.Entities
{
    public class Provider : IEquatable<Provider>
    {
        [Key]
        [Required]
        public int ID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public ProviderType Type { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Namespace { get; set; }

        [Required]
        public string Version { get; set; }

        public string? GenericType { get; set; }
        
        public List<ProviderProperty> Properties { get; set; }

        #region Overrides
        public override bool Equals(object obj)
        {
            return Equals(obj as Provider);
        }

        public bool Equals(Provider other)
        {
            return Equals(other, true);
        }

        public bool Equals(Provider other, bool includeId)
        {
            if (other == null)
                return false;

            if (includeId && ID != other.ID)
                return false;

            if (Name != other.Name)
                return false;

            if (Type != other.Type)
                return false;

            if (Namespace != other.Namespace)
                return false;

            if (FullName != other.FullName)
                return false;

            if (Version != other.Version)
                return false;

            if (GenericType != other.GenericType)
                return false;

            return true;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ID, Name, Type, FullName, Namespace, Version, GenericType);
        }
        #endregion
    }
}
