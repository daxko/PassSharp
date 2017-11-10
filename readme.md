# PassSharp

[![Travis CIBuild Status](https://travis-ci.org/daxko/PassSharp.svg?branch=master)](https://travis-ci.org/daxko/PassSharp) [![AppVeyor Build Status](https://ci.appveyor.com/api/projects/status/7xm3msq279h5pgah/branch/master?svg=true)](https://ci.appveyor.com/project/daxko/PassSharp) [![NuGet](https://img.shields.io/nuget/v/PassSharp.svg)](https://www.nuget.org/packages/PassSharp/)

An Apple Wallet Passbook Library in C#/.NET supporting iOS 6+

## Install PassSharp

`PM> Install-Package PassSharp`

*or*

`$ nuget install PassSharp`

## Creating Passes

You will need to use the `Pass` class to set the necessary fields for the type of pass you want to create. (See Apple's [Pass Design and Creation documentation](https://developer.apple.com/library/content/documentation/UserExperience/Conceptual/PassKit_PG/Creating.html))

```
var pass = new Pass {
  type = PassType.generic,
  passTypeIdentifier = "pass.com.my.pass",
  description = "example pass",
  organizationName = "acme corp",
  serialNumber = "abc123456",
  teamIdentifier = "U1234567",
  icon = new Asset("path/to/icon"),
  icon2x = new Asset("path/to/icon2x"),
  icon3x = new Asset("path/to/icon3x"),
  logo = new Asset("path/to/logo"),
  logo2x = new Asset("path/to/logo2x"),
  logo3x = new Asset("path/to/logo3x")
};
```

You can now set more general fields (such as location, barcode, etc.).

```
pass.AddLocation({ longitude: 10.0000, latitude: -10.0000 });
pass.AddBarcode(new Barcode {
  message = "1234",
  format = BarcodeFormat.PKBarcodeFormatPDF417
});
pass.AddField(FieldType.Header,
  new Field {
    key = "pass-field",
    label = "pass-field-label",
    value = "pass-field-value"
  }
);
pass.AddField(FieldType.Primary,
  new Field {
    key = "pass-field2",
    label = "pass-field-label2",
    value = "pass-field-value2"
  }
);
```

## Writing Passes & Certificates

In order to create passes you will need to download the [Apple WWDR intermediate certificate](http://developer.apple.com/certificationauthority/AppleWWDRCA.cer), and to [generate your own pass id certificate](https://developer.apple.com/library/content/documentation/IDEs/Conceptual/AppDistributionGuide/MaintainingCertificates/MaintainingCertificates.html#//apple_ref/doc/uid/TP40012582-CH31-SW32) from the [Apple developers portal](http://developer.apple.com/account).

You can then write passes by including the Apple WWDR certificate, and the pass id certificate that you previously generated.

```
var myPass = new Pass { ... };
using(var myStream = new MemoryStream()) {
  PassWriter.Write(myPass, myStream,
    new X509Certificate2("../path/to/appleWWDRCertificate"),
    new X509Certificate2("../path/to/passCertificate"));
}
```
