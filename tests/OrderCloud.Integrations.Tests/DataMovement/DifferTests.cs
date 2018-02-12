using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OrderCloud.Integrations.DataMovement;

namespace OrderCloud.Integrations.Tests.DataMovement
{
	public class DifferTests
	{
		[Test, Ignore("just demonstrating for now")]
		public async Task can_diff_large_set() {
			var differ = new Differ<Product>("localhost", "product-sync");
			differ.ParseRow = s => new Product {
				Id = s.Split(',')[0],
				Name = s.Split(',')[1],
				Price = decimal.Parse(s.Split(',')[2])
			};
			differ.GetId = p => p.Id;

			var file = new CsvFile();
			var db = new FakeDatabase();

			await differ.LoadCurrentAsync(file.TimeStamp, file.GetRows());

			foreach (var diff in await differ.GetDiffsAsync()) {
				if (diff.ChangeType == ChangeType.Create)
					db.Create(diff.Current);
				else if (diff.ChangeType == ChangeType.Update)
					db.Update(diff.Previous.Id, diff.Current);
				else
					db.Delete(diff.Previous.Id);
			}
		}

		class Product
		{
			public string Id { get; set; }
			public string Name { get; set; }
			public decimal Price { get; set; }
		}

		class FakeDatabase
		{
			public void Create(object obj) { }
			public void Update(string id, object obj) { }
			public void Delete(string id) { }
		}

		class CsvFile
		{
			public string TimeStamp { get; set; }

			public IEnumerable<string> GetRows() {
				yield break;
			}
		}
	}
}
