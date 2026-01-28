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
            var esh3arGroup = context.AddGroup($"{AppPrefix}", L("AppName"));

            esh3arGroup.AddFeature($"{AppPrefix}.Chat", defaultValue: "false",
                displayName: L("Feature:Chat"),
                valueType: new ToggleStringValueType()
                );
            
            esh3arGroup.AddFeature($"{AppPrefix}.AutomaticReply", defaultValue: "false",
                displayName: L("Feature:AutomaticReply"),
                valueType: new ToggleStringValueType()
                );

            esh3arGroup.AddFeature($"{AppPrefix}.PdfReporting", defaultValue: "false", 
                displayName: L("Feature:PdfReporting"), 
                valueType: new ToggleStringValueType()
                );
            
            esh3arGroup.AddFeature($"{AppPrefix}.ExcelReporting", defaultValue: "false",
                displayName: L("Feature:ExcelReporting"),
                valueType: new ToggleStringValueType()
                );

            esh3arGroup.AddFeature(
                $"{AppPrefix}.MaxMessages", defaultValue: "50",
                displayName: L("Feature:MaxMessages"),
                valueType: new FreeTextStringValueType(new NumericValueValidator())
                );
        }

        private static LocalizableString L(string name)
        {
            return LocalizableString.Create<Esh3arTechResource>(name);
        }
    }
}
