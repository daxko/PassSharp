using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Machine.Specifications;
using PassSharp;
using PassSharp.Fields;

#pragma warning disable 414

namespace Test
{

  [Subject("Pass Fields")]
  public class when_adding_fields
  {
    static Pass pass;

    Establish context = () => {
      pass = new Pass();
    };

    It should_not_have_empty_fields = () => pass.fields.ShouldNotBeNull();
    It should_not_have_empty_beacons = () => pass.beacons.ShouldNotBeNull();
    It should_not_have_empty_locations = () => pass.locations.ShouldNotBeNull();
    It should_not_have_empty_barcodes = () => pass.barcodes.ShouldNotBeNull();
    It should_not_have_empty_nfcs = () => pass.nfc.ShouldNotBeNull();

    public class when_adding_a_field_type
    {
      Because of = () => pass.AddField(FieldType.Header, new Field { });
      It should_have_one_field = () => pass.fields.Count.ShouldEqual(1);
      It should_have_correct_field_type = () => new List<FieldType>(pass.fields.Keys).ElementAt(0).ShouldEqual(FieldType.Header);
    }

    public class when_adding_multiple_fields_of_same_type
    {
      Because of = () => {
        pass.AddField(FieldType.Header, new Field { });
        pass.AddField(FieldType.Header, new Field { });
      };
      It should_have_multiple_fields = () => pass.fields[FieldType.Header].Count.ShouldEqual(2);
    }

    public class when_adding_multiple_fields_of_different_types
    {
      Because of = () => {
        pass.AddField(FieldType.Header, new Field { });
        pass.AddField(FieldType.Primary, new Field { });
      };
      It should_have_multiple_fields = () => pass.fields.Count.ShouldEqual(2);
    }

    public class when_adding_a_beacon
    {
      Because of = () => pass.AddBeacon(new Beacon { });
      It should_have_one_beacon = () => pass.beacons.Count.ShouldEqual(1);
    }

    public class when_adding_multiple_beacons
    {
      Because of = () => {
        pass.AddBeacon(new Beacon { });
        pass.AddBeacon(new Beacon { });
      };
      It should_have_multiple_beacons = () => pass.beacons.Count.ShouldEqual(2);
    }

    public class when_adding_a_location
    {
      Because of = () => pass.AddLocation(new Location { });
      It should_have_one_location = () => pass.locations.Count.ShouldEqual(1);
    }

    public class when_adding_multiple_locations
    {
      Because of = () => {
        pass.AddLocation(new Location { });
        pass.AddLocation(new Location { });
      };
      It should_have_multiple_locations = () => pass.locations.Count.ShouldEqual(2);
    }

    public class when_adding_a_barcode
    {
      Because of = () => pass.AddBarcode(new Barcode { });
      It should_have_one_barcode = () => pass.barcodes.Count.ShouldEqual(1);
    }

    public class when_adding_multiple_barcodes
    {
      Because of = () => {
        pass.AddBarcode(new Barcode { });
        pass.AddBarcode(new Barcode { });
      };
      It should_have_multiple_barcodes = () => pass.barcodes.Count.ShouldEqual(2);
    }

    public class when_adding_a_nfc
    {
      Because of = () => pass.AddNFC(new NFC { });
      It should_have_one_nfc = () => pass.nfc.Count.ShouldEqual(1);
    }

    public class when_adding_multiple_nfcs
    {
      Because of = () => {
        pass.AddNFC(new NFC { });
        pass.AddNFC(new NFC { });
      };
      It should_have_multiple_nfcs = () => pass.nfc.Count.ShouldEqual(2);
    }

    public class when_adding_a_localization
    {
      Because of = () => pass.AddLocalization(new Localization(CultureInfo.CurrentCulture));
      It should_have_one_localization = () => pass.localizations.Count.ShouldEqual(1);
    }

    public class when_adding_multiple_localizations
    {
      Because of = () => {
        pass.AddLocalization(new Localization(CultureInfo.CurrentCulture));
        pass.AddLocalization(new Localization(CultureInfo.CurrentCulture));
      };
      It should_have_multiple_localizations = () => pass.localizations.Count.ShouldEqual(2);
    }

  }

}
