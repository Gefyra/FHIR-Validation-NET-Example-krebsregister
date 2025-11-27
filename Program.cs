using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Specification.Source;
using Hl7.Fhir.Specification.Terminology;
using Hl7.Fhir.Validation;
using Validator = Hl7.Fhir.Validation.Validator;

class Program
{

    public static void Main(string[] args)
    {
        CommonZipSource igSourceDeBasis = new CommonZipSource(ModelInfo.ModelInspector, "de.basisprofil.r4#1.5.4.zip","./", new DirectorySourceSettings()
            {
                IncludeSubDirectories = true
            });
        
        CommonZipSource igSourceStf = new CommonZipSource(ModelInfo.ModelInspector,
            "de.gematik.sterbefall#1.0.0-beta.3.zip", "./", new DirectorySourceSettings()
            {
                IncludeSubDirectories = true
            });

        //ZipSource igSourceStf = new ZipSource("de.gematik.sterbefall#1.0.0-beta.3.zip");

        //ZipSource igSourceDeBasis = new ZipSource("de.basisprofil.r4#1.5.4.zip");

        CachedResolver resourceResolver = new CachedResolver(
            new MultiResolver(igSourceStf, igSourceDeBasis, ZipSource.CreateValidationSource()));
        LocalTerminologyService terminology = new LocalTerminologyService(resourceResolver);

        FhirJsonParser parser = new FhirJsonParser();
        Bundle bundle = parser.Parse<Bundle>(File.ReadAllText("Bundle-StfExportBundle-BY1.json"));
        string profile = "http://gematik.de/fhir/oegd/stf/StructureDefinition/StfExportBundle";
        
        ValidationSettings settings = ValidationSettings.CreateDefault();
        settings.ResourceResolver   = resourceResolver;
        settings.TerminologyService = terminology;

        Validator validator = new Validator(settings);
        OperationOutcome outcome = validator.Validate(bundle, profile);

        foreach (OperationOutcome.IssueComponent issue in outcome.Issue)
        {
            Console.WriteLine($"{issue.Severity}: {issue.Details?.Text}");
        }
    }
    
}