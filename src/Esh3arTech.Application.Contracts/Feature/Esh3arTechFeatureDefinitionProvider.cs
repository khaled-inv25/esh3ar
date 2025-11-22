using Esh3arTech.Localization;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.Validation.StringValues;

namespace Esh3arTech.Feature
{
    public class Esh3arTechFeatureDefinitionProvider : FeatureDefinitionProvider
    {
        private const string AppPrefix = "Esh3arTech";

        // To Define all esh3ar tech features.
        public override void Define(IFeatureDefinitionContext context)
        {
            var esh3arGroup = context.AddGroup($"{AppPrefix}", LocalizableString.Create<Esh3arTechResource>(AppPrefix));

            esh3arGroup.AddFeature($"{AppPrefix}.Chat", defaultValue: "false",
                displayName: LocalizableString.Create<Esh3arTechResource>("Chat"),
                valueType: new ToggleStringValueType()
                );

            esh3arGroup.AddFeature($"{AppPrefix}.PdfReporting", defaultValue: "false", 
                LocalizableString.Create<Esh3arTechResource>("PdfReporting"), 
                valueType: new ToggleStringValueType()
                );
            
            esh3arGroup.AddFeature($"{AppPrefix}.ExcelReporting", defaultValue: "false",
                displayName: LocalizableString.Create<Esh3arTechResource>("ExcelReporting"),
                valueType: new ToggleStringValueType()
                );

            esh3arGroup.AddFeature(
                $"{AppPrefix}.MaxMessages", defaultValue: "50",
                displayName: LocalizableString.Create<Esh3arTechResource>("MaxMessages"),
                valueType: new FreeTextStringValueType(new NumericValueValidator())
                );

        }
    }
}
