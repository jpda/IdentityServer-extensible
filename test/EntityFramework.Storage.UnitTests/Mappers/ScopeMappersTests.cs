// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using System;
using System.Linq;
using System.Reflection;
using Duende.IdentityServer.EntityFramework.Mappers;
using Models = Duende.IdentityServer.Models;
using Entities = Duende.IdentityServer.EntityFramework.Entities;
using FluentAssertions;
using Xunit;
using System.Collections.Generic;

namespace UnitTests.Mappers;

public class ScopesMappersTests
{
    [Fact]
    public void CanMapScope()
    {
        var model = new Models.ApiScope();
        var mappedEntity = model.ToEntity();
        var mappedModel = mappedEntity.ToModel();

        Assert.NotNull(mappedModel);
        Assert.NotNull(mappedEntity);
    }

    [Fact]
    public void Mapping_model_to_entity_maps_all_properties()
    {
        var source = new Models.ApiScope()
        {
            Description = "description",
            DisplayName = "displayname",
            Name = "foo",
            UserClaims = { "c1", "c2" },
            Properties = {
                { "x", "xx" },
                { "y", "yy" },
            },
            Enabled = false
        };

        var excludedProperties = new string[]
        {
            "Updated",
            "Created",
            "LastAccessed",
            "NonEditable"
        };

        var destination = source.ToEntity();

        MapperTestHelpers
            .AllPropertiesAreMapped(source, destination, excludedProperties, out var unmappedMembers)
            .Should()
            .BeTrue($"{string.Join(',', unmappedMembers)} should be mapped");
    }

    [Fact]
    public void Mapping_entity_to_model_maps_all_properties()
    {
        var source = new Entities.ApiScope()
        {
            Description = "description",
            DisplayName = "displayname",
            Name = "foo",
            UserClaims = new List<Entities.ApiScopeClaim> { 
                new Entities.ApiScopeClaim
                {
                    Type = "c1",  
                },
                new Entities.ApiScopeClaim
                {
                    Type = "c2"
                }
            },
            Properties = new List<Entities.ApiScopeProperty>  {
                new Entities.ApiScopeProperty
                {
                    Key = "x",
                    Value = "xx" 
                },
                new Entities.ApiScopeProperty
                {
                    Key = "y",
                    Value = "yy" 
                },
            },
            Enabled = false
        };

        var excludedProperties = new string[]
        {
        };

        var destination = source.ToModel();

        MapperTestHelpers
            .AllPropertiesAreMapped(source, destination, excludedProperties, out var unmappedMembers)
            .Should()
            .BeTrue($"{string.Join(',', unmappedMembers)} should be mapped");
    }

    [Fact]
    public void Properties_Map()
    {
        var model = new Duende.IdentityServer.Models.ApiScope()
        {
            Description = "description",
            DisplayName = "displayname",
            Name = "foo",
            UserClaims = { "c1", "c2" },
            Properties = {
                { "x", "xx" },
                { "y", "yy" },
            },
            Enabled = false
        };


        var mappedEntity = model.ToEntity();
        mappedEntity.Description.Should().Be("description");
        mappedEntity.DisplayName.Should().Be("displayname");
        mappedEntity.Name.Should().Be("foo");

        mappedEntity.UserClaims.Count.Should().Be(2);
        mappedEntity.UserClaims.Select(x => x.Type).Should().BeEquivalentTo(new[] { "c1", "c2" });
        mappedEntity.Properties.Count.Should().Be(2);
        mappedEntity.Properties.Should().Contain(x => x.Key == "x" && x.Value == "xx");
        mappedEntity.Properties.Should().Contain(x => x.Key == "y" && x.Value == "yy");


        var mappedModel = mappedEntity.ToModel();

        mappedModel.Description.Should().Be("description");
        mappedModel.DisplayName.Should().Be("displayname");
        mappedModel.Enabled.Should().BeFalse();
        mappedModel.Name.Should().Be("foo");
        mappedModel.UserClaims.Count.Should().Be(2);
        mappedModel.UserClaims.Should().BeEquivalentTo(new[] { "c1", "c2" });
        mappedModel.Properties.Count.Should().Be(2);
        mappedModel.Properties["x"].Should().Be("xx");
        mappedModel.Properties["y"].Should().Be("yy");
    }
}