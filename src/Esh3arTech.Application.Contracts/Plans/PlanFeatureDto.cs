using Volo.Abp.Validation.StringValues;

namespace Esh3arTech.Plans
{
    // For group of plan features.
    public class PlanFeatureDto
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public string Value { get; set; }

        public string Description { get; set; }

        public IStringValueType ValueType { get; set; }
    }
}
