// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.VisualStudio.Web.CodeGeneration.EntityFrameworkCore
{
    public class PropertyMetadata
    {
        /// <summary>
        /// Use this constructor when the model is being used without datacontext.
        /// It will set the property as:
        ///   Non primary 
        ///   Non foreign key.
        ///   Non autogenerated
        ///   Writable
        ///   Non Enum type
        /// </summary>
        /// <param name="property"></param>
        public PropertyMetadata(PropertyInfo property)
        {
            Contract.Assert(property != null && property.Name != null && property.PropertyType != null);
            PropertyName = property.Name;
            TypeName = property.PropertyType.FullName;

            ShortTypeName = TypeUtil.GetShortTypeName(property.PropertyType);
            Scaffold = true;
            var scaffoldAttr = property.GetCustomAttribute(typeof(ScaffoldColumnAttribute)) as ScaffoldColumnAttribute;
            if (scaffoldAttr != null && !scaffoldAttr.Scaffold)
            {
                Scaffold = false;
            }

            // Since this is not being treated as an EF based model,
            // below values are set as false.
            IsPrimaryKey = false;
            IsForeignKey = false;
            IsEnum = false; 
            IsEnumFlags = false;
            IsReadOnly = false;
            IsAutoGenerated = false;
        }

        public PropertyMetadata(IProperty property, Type dbContextType)
        {
            Contract.Assert(property != null);
            Contract.Assert(property.ClrType != null); //Do we need to make sure this is not called on Shadow properties?

            PropertyName = property.Name;
            TypeName = property.ClrType.FullName;

            IsPrimaryKey = property.IsPrimaryKey();
            // The old scaffolding has some logic for this property in an edge case which is
            // not clear if needed any more; see EntityFrameworkColumnProvider.DetermineIsForeignKeyComponent
            IsForeignKey = property.IsForeignKey();

            IsEnum = property.ClrType.GetTypeInfo().IsEnum;
            IsReadOnly = property.IsReadOnlyBeforeSave;
            IsAutoGenerated = property.IsStoreGeneratedAlways;

            ShortTypeName = TypeUtil.GetShortTypeName(property.ClrType);

            Scaffold = true;
            var reflectionProperty = property.DeclaringEntityType.ClrType.GetProperty(property.Name);
            if (reflectionProperty != null)
            {
                var scaffoldAttr = reflectionProperty.GetCustomAttribute(typeof(ScaffoldColumnAttribute)) as ScaffoldColumnAttribute;
                if (scaffoldAttr != null)
                {
                    Scaffold = scaffoldAttr.Scaffold;
                }
            }

            IsEnumFlags = false;
            if (IsEnum)
            {
                var flagsAttr = property.ClrType.GetTypeInfo().GetCustomAttribute(typeof(FlagsAttribute)) as FlagsAttribute;
                IsEnumFlags = (flagsAttr != null);
            }
        }

        public bool IsAutoGenerated { get; set; }

        public bool IsEnum { get; set; }

        public bool IsEnumFlags { get; set; }

        public bool IsForeignKey { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsReadOnly { get; set; }

        public string PropertyName { get; set; }

        public bool Scaffold { get; set; }

        public string ShortTypeName { get; set; }

        public string TypeName { get; set; }
    }
}