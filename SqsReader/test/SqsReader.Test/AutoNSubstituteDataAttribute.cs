using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SqsReader.Test
{
    public class AutoNSubstituteDataAttribute : AutoDataAttribute
    {
        public AutoNSubstituteDataAttribute()
            : base(CreateNewFixture) { }

        private static IFixture CreateNewFixture()
        {
            var fixture = new Fixture();
            // https://github.com/AutoFixture/AutoFixture/issues/1141
            fixture.Customize<BindingInfo>(c => c.OmitAutoProperties());
            return fixture.Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
        }
    }
}