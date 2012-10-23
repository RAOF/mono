// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Xunit;

    public class StringExtensionsTests
    {
        [Fact]
        public void Break_should_break_long_strings()
        {
            var s = new string('a', 3000);
            var lines = StringExtensions.Break(s);

            Assert.Equal(3, lines.Count());
            Assert.Equal(s, string.Join(string.Empty, lines));

            s = new string('a', 3001);

            lines = StringExtensions.Break(s);

            Assert.Equal(4, lines.Count());
            Assert.Equal(s, string.Join(string.Empty, lines));

            s = new string('a', 2999);

            lines = StringExtensions.Break(s);

            Assert.Equal(3, lines.Count());
            Assert.Equal(s, string.Join(string.Empty, lines));

            s = new string('a', 30);

            lines = StringExtensions.Break(s, width: 1);

            Assert.Equal(30, lines.Count());
            Assert.Equal(s, string.Join(string.Empty, lines));
        }

        [Fact]
        public void EqualsIgnoreCase_should_ignore_case()
        {
            Assert.True(StringExtensions.EqualsIgnoreCase("foo", "Foo"));
            Assert.False(StringExtensions.EqualsIgnoreCase("Bar", "Foo"));
        }

        [Fact]
        public void IsValid_should_correctly_validate_ids()
        {
            Assert.False(StringExtensions.IsValidMigrationId("Foo"));
            Assert.False(StringExtensions.IsValidMigrationId("11111111111111_Foo"));
            Assert.False(StringExtensions.IsValidMigrationId("11111111111111Foo"));
            Assert.True(StringExtensions.IsValidMigrationId("111111111111111_Foo"));
            Assert.True(StringExtensions.IsValidMigrationId(DbMigrator.InitialDatabase));
        }

        [Fact]
        public void IsAutomatic_detects_automatic_migration_names()
        {
            Assert.True(StringExtensions.IsAutomaticMigration("111111111111111_AutomaticMigration"));
            Assert.True(StringExtensions.IsAutomaticMigration("111111111111111_Foo_AutomaticMigration"));
            Assert.False(StringExtensions.IsAutomaticMigration("111111111111111_Foo"));
        }

        [Fact]
        public void RestrictTo_should_limit_string_length()
        {
            Assert.Equal("123", StringExtensions.RestrictTo("123", 5));
            Assert.Equal("123", StringExtensions.RestrictTo("123", 3));
            Assert.Equal("123", StringExtensions.RestrictTo("12345", 3));
            Assert.Equal("", StringExtensions.RestrictTo("12345", 0));
            Assert.Equal("", StringExtensions.RestrictTo("", 0));
            Assert.Equal(null, StringExtensions.RestrictTo(null, 0));
        }

        [Fact]
        public void ToAutomaticMigrationId_should_rewind_timestamp_and_append_auto_string()
        {
            Assert.Equal("201205054534555_Foo_AutomaticMigration", StringExtensions.ToAutomaticMigrationId("201205054534556_Foo"));
            Assert.Equal("111111111111109_Foo_AutomaticMigration", StringExtensions.ToAutomaticMigrationId("111111111111110_Foo"));
        }
    }
}
