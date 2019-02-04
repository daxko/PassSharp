using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Cms;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509.Store;
using PassSharp.Fields;
using ServiceStack;
using ServiceStack.Text;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace PassSharp
{

	public class PassWriter
	{
		private ZipArchive archive;

		public void WriteToStream(Pass pass, Stream stream, X509Certificate2 appleCert, X509Certificate2 passCert)
		{
			using (archive = new ZipArchive(stream, ZipArchiveMode.Update, true)) {
				AddEntry(@"pass.json", ToJson(pass));

				AddAssetEntry(@"icon.png", pass.icon);
				AddAssetEntry(@"icon@2x.png", pass.icon2x);
				AddAssetEntry(@"icon@3x.png", pass.icon3x);
				AddAssetEntry(@"logo.png", pass.logo);
				AddAssetEntry(@"logo@2x.png", pass.logo2x);
				AddAssetEntry(@"logo@3x.png", pass.logo3x);
				AddAssetEntry(@"background.png", pass.background);
				AddAssetEntry(@"background@2x.png", pass.background2x);
				AddAssetEntry(@"background@3x.png", pass.background3x);
				AddAssetEntry(@"footer.png", pass.footer);
				AddAssetEntry(@"footer@2x.png", pass.footer2x);
				AddAssetEntry(@"footer@3x.png", pass.footer3x);
				AddAssetEntry(@"strip.png", pass.strip);
				AddAssetEntry(@"strip@2x.png", pass.strip2x);
				AddAssetEntry(@"strip@3x.png", pass.strip3x);
				AddAssetEntry(@"thumbnail.png", pass.thumbnail);
				AddAssetEntry(@"thumbnail@2x.png", pass.thumbnail2x);
				AddAssetEntry(@"thumbnail@3x.png", pass.thumbnail3x);

				if (pass.localizations != null && pass.localizations.Count > 0) {
					foreach (var localization in pass.localizations) {
						Func<string, string> entryName = name => @"{0}/{1}".FormatWith("{0}.lproj".FormatWith(localization.culture), name);

						var passStrings = new List<string>();
						if (localization.values != null) {
							foreach (var key in localization.values.Keys) {
								passStrings.Add(@"""{0}"" = ""{1}"";".FormatWith(key, localization.values[key]));
							}
							AddEntry(entryName("pass.strings"), passStrings.Join("\n"));
						}

						AddAssetEntry(entryName("icon.png"), localization.icon);
						AddAssetEntry(entryName("icon@2x.png"), localization.icon2x);
						AddAssetEntry(entryName("icon@3x.png"), localization.icon3x);
						AddAssetEntry(entryName("logo.png"), localization.logo);
						AddAssetEntry(entryName("logo@2x.png"), localization.logo2x);
						AddAssetEntry(entryName("logo@3x.png"), localization.logo3x);
						AddAssetEntry(entryName("background.png"), localization.background);
						AddAssetEntry(entryName("background@2x.png"), localization.background2x);
						AddAssetEntry(entryName("background@3x.png"), localization.background3x);
						AddAssetEntry(entryName("footer.png"), localization.footer);
						AddAssetEntry(entryName("footer@2x.png"), localization.footer2x);
						AddAssetEntry(entryName("footer@3x.png"), localization.footer3x);
						AddAssetEntry(entryName("strip.png"), localization.strip);
						AddAssetEntry(entryName("strip@2x.png"), localization.strip2x);
						AddAssetEntry(entryName("strip@3x.png"), localization.strip3x);
						AddAssetEntry(entryName("thumbnail.png"), localization.thumbnail);
						AddAssetEntry(entryName("thumbnail@2x.png"), localization.thumbnail2x);
						AddAssetEntry(entryName("thumbnail@3x.png"), localization.thumbnail3x);
					}
				}

				var manifestJson = GenerateManifest().ToJson();
				AddEntry(@"manifest.json", manifestJson);
				AddEntry(@"signature", GenerateSignature(manifestJson.ToUtf8Bytes(), appleCert, passCert));
			}
		}

		public void WriteToFile(Pass pass, string path, X509Certificate2 appleCert, X509Certificate2 passCert)
		{
			using (var stream = new FileStream(path, FileMode.OpenOrCreate)) {
				WriteToStream(pass, stream, appleCert, passCert);
			}
		}

		protected Dictionary<string, string> GenerateManifest()
		{
			var hashManifest = new Dictionary<string, string>();

			foreach (var entry in archive.Entries) {
				hashManifest.Add(entry.FullName, CalculateSHA1(entry.Open()));
			}

			return hashManifest;
		}

		protected byte[] GenerateSignature(byte[] manifest, X509Certificate2 appleCert, X509Certificate2 passCert)
		{
			X509Certificate apple = DotNetUtilities.FromX509Certificate(appleCert);
			X509Certificate cert = DotNetUtilities.FromX509Certificate(passCert);

			var privateKey = DotNetUtilities.GetKeyPair(passCert.PrivateKey).Private;
			var generator = new CmsSignedDataGenerator();

			generator.AddSigner(privateKey, cert, CmsSignedGenerator.DigestSha1);

			var list = new List<X509Certificate>();
			list.Add(cert);
			list.Add(apple);

			X509CollectionStoreParameters storeParameters = new X509CollectionStoreParameters(list);
			IX509Store store509 = X509StoreFactory.Create("CERTIFICATE/COLLECTION", storeParameters);

			generator.AddCertificates(store509);

			var content = new CmsProcessableByteArray(manifest);
			var signature = generator.Generate(content, false).GetEncoded();

			return signature;
		}

		protected void AddEntry(string name, string value)
		{
			AddEntry(name, value.ToUtf8Bytes());
		}

		protected void AddEntry(string name, byte[] value)
		{
			using (var entry = archive.CreateEntry(name).Open()) {
				entry.Write(value, 0, value.Length);
			}
		}

		protected void AddFileEntry(string name, string filename)
		{
			AddEntry(name, File.ReadAllBytes(filename));
		}

		protected void AddAssetEntry(string name, Asset asset)
		{
			if (null != asset) {
				AddEntry(name, asset.asset);
			}
		}

		protected string CalculateSHA1(Stream stream)
		{
			using (SHA1Managed managed = new SHA1Managed()) {
				byte[] checksum = managed.ComputeHash(stream);
				return BitConverter.ToString(checksum).Replace("-", string.Empty).ToLower();
			}
		}

	  public string ToJson(Pass pass)
		{
			var properties = pass.GetType().GetProperties();
			var jsonDict = new Dictionary<object, object>();

			Func<string, bool> isIgnoredField = value => new List<string> { "type", "localizations" }.Contains(value);
			Func<object, bool> isNull = value => value == null;
			Func<object, bool> isAsset = value => value.GetType() == typeof(Asset);
			Func<object, bool> isEmptyList = value => value is IList && ((IList)value).Count == 0;

			foreach (var property in properties) {
				string name = property.Name;
				object value = property.GetValue(pass);

				if (name.Equals("fields")) {
					var fields = ((Dictionary<FieldType, List<Field>>)value)
						.ToDictionary(x => SerializeFieldType(x.Key), x => x.Value);
					jsonDict.Add(pass.type, fields);
				} else if (isIgnoredField(name) || isNull(value) || isAsset(value) || isEmptyList(value)) {
					// don't include value
				} else {
					jsonDict.Add(name, value);
				}

			}

			string json = null;
			using (var config = JsConfig.CreateScope("ExcludeTypeInfo")) {
				json = jsonDict.ToJson();
			}

			return json;
		}

		protected Func<FieldType, string> SerializeFieldType = (value) => {
			switch (value) {
				case FieldType.Auxiliary:
					return "auxiliaryFields";
				case FieldType.Back:
					return "backFields";
				case FieldType.Header:
					return "headerFields";
				case FieldType.Primary:
					return "primaryFields";
				case FieldType.Secondary:
					return "secondaryFields";
				default:
					return "";
			}
		};

	}
}
