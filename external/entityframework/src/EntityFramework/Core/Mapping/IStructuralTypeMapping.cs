// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.ObjectModel;

    internal interface IStructuralTypeMapping
    {
        ReadOnlyCollection<StoragePropertyMapping> Properties { get; }

        void AddProperty(StoragePropertyMapping propertyMapping);
        void RemoveProperty(StoragePropertyMapping propertyMapping);
    }
}
