using Ribosoft.Models;
using Ribosoft.Models.RequestViewModels;
using Ribosoft.Models.RibozymeViewModel;
using Ribosoft.ValidationAttributes;
using System.Collections.Generic;
using Xunit;

namespace Ribosoft.Tests
{
    public class TestValidationAttributes
    {
        [Fact]
        public void TestNucleotideAttribute()
        {
            NucleotideAttribute attribute = new NucleotideAttribute();
            Assert.True(attribute.IsValid('A'));
            Assert.False(attribute.IsValid('Q'));
            Assert.Equal("Element must only contain the following upper or lower case values: A, C, G, U, T, R, Y, K, M, S, W, B, D, H, V, N", attribute.FormatErrorMessage("Element"));
        }

        [Fact]
        public void TestOpenReadingFrameStartAttribute()
        {
            OpenReadingFrameAttribute attribute = new OpenReadingFrameAttribute();
            Assert.True(attribute.IsValid(1));
            Assert.False(attribute.IsValid(-1));
            Assert.Equal("Index must be a positive integer", attribute.FormatErrorMessage("Element"));
        }

        [Fact]
        public void TestRepeatNotationsAttribute()
        {
            RepeatNotationsAttribute attribute = new RepeatNotationsAttribute(5);
            Assert.True(attribute.IsValid("nnAUG"));
            Assert.False(attribute.IsValid("nnnnnn"));
            Assert.Equal("Amount of repeat notations cannot exceed 5", attribute.FormatErrorMessage("Element"));
        }

        [Fact]
        public void TestUniqueAlphaNumericStructureAttribute()
        {
            UniqueAlphaNumericStructureAttribute attribute = new UniqueAlphaNumericStructureAttribute();
            Assert.True(attribute.IsValid("..(()).."));
            Assert.True(attribute.IsValid("..1.2.3"));
            Assert.False(attribute.IsValid(".aa.352"));
            Assert.False(attribute.IsValid(".{.}."));
            Assert.Equal("Alphanumerics within the structure must only occur once", attribute.FormatErrorMessage("Element"));
        }

        [Fact]
        public void TestValidStructureAttribute()
        {
            ValidStructureAttribute attribute = new ValidStructureAttribute();
            Assert.True(attribute.IsValid("..([]).."));
            Assert.False(attribute.IsValid("...("));
            Assert.False(attribute.IsValid("...)"));
            Assert.False(attribute.IsValid("...["));
            Assert.False(attribute.IsValid("...]"));

            attribute = new ValidStructureAttribute();
            Assert.Equal("Invalid structure format, ensure all bonds and pseudo knots have matching closing symbols", attribute.FormatErrorMessage("Element"));
        }

        [Fact]
        public void TestValidateRequestAttribute()
        {
            ValidateRequestAttribute attribute = new ValidateRequestAttribute();

            RequestViewModel vm = new RequestViewModel();
            vm.OpenReadingFrameStart = 0;
            vm.OpenReadingFrameEnd = 5;
            vm.InputSequence = "AUGACAG";
            Assert.True(attribute.IsValid(vm));

            RequestViewModel vm1 = new RequestViewModel();
            vm1.OpenReadingFrameStart = 10;
            vm1.OpenReadingFrameEnd = 5;
            vm1.InputSequence = "AUGACAG";
            Assert.False(attribute.IsValid(vm1));

            RequestViewModel vm2 = new RequestViewModel();
            vm2.OpenReadingFrameStart = 0;
            vm2.OpenReadingFrameEnd = 50;
            vm2.InputSequence = "AUGACAG";
            Assert.False(attribute.IsValid(vm2));
            
            RequestViewModel vm3 = new RequestViewModel();
            vm3.OpenReadingFrameStart = 40;
            vm3.OpenReadingFrameEnd = 50;
            vm3.InputSequence = "AUGACAG";
            Assert.False(attribute.IsValid(vm3));

            Assert.Equal("Invalid start and end index, verify end position is after start and within the sequence", attribute.FormatErrorMessage("Element"));
        }

        [Fact]
        public void TestValidateRibozymeStructureAttribute()
        {
            ValidateRibozymeStructureAttribute attribute = new ValidateRibozymeStructureAttribute();

            RibozymeStructure structure = new RibozymeStructure();
            structure.Sequence = "AUGCAUGC";
            structure.Structure = "a.((..))";
            structure.SubstrateTemplate = "AGUC";
            structure.SubstrateStructure = "a...";
            structure.Cutsite = 2;
            Assert.True(attribute.IsValid(structure));

            structure.SubstrateStructure = "....";
            Assert.False(attribute.IsValid(structure));

            structure.SubstrateStructure = "..";
            Assert.False(attribute.IsValid(structure));

            structure.Sequence = "AUGUAAGUAGCUGAC";
            Assert.False(attribute.IsValid(structure));

            structure.Cutsite = 100;
            Assert.False(attribute.IsValid(structure));

            RibozymeCreateViewModel vm = new RibozymeCreateViewModel();
            vm.RibozymeStructures = new List<RibozymeStructure>();
            vm.RibozymeStructures.Add(structure);
            Assert.False(attribute.IsValid(vm));

            vm.RibozymeStructures.Remove(structure);
            structure.Sequence = "AUGCAUGC";
            structure.Structure = "a.((..))";
            structure.SubstrateTemplate = "AGUC";
            structure.SubstrateStructure = "a...";
            structure.Cutsite = 2;
            vm.RibozymeStructures.Add(structure);
            Assert.True(attribute.IsValid(vm));

            Assert.Equal("Invalid input, ensure sequence and substrates match their structures", attribute.FormatErrorMessage("Element"));
        }
    }
}
