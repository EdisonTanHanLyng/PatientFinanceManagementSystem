using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using PFMS_MI04.Models;
using PFMS_MI04.Controllers;
using System.Text.RegularExpressions;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using PFMS_MI04.Models;
using System.Text.Json;
using RestSharp;

namespace PFMS_MI04.Testing
{
    public class SponsorListTest
    {
        private List<SponsorListItem> searchResults;
        private List<SponsorListItem> sponsorAllLists = new List<SponsorListItem>();
        private readonly string _mockJsonData;

        public SponsorListTest()
        {
            _mockJsonData = @"{
                ""sponsor1"": {
                    ""sponsor_id"": 1001,
                    ""sponsor_name"": ""Tech Innovations Inc."",
                    ""office_phone"": ""555-0101"",
                    ""contact_person"": ""John Doe"",
                    ""sponsor_code"": ""TII001""
                },
                ""sponsor2"": {
                    ""sponsor_id"": 1002,
                    ""sponsor_name"": ""Global Solutions Ltd."",
                    ""office_phone"": ""555-0202"",
                    ""contact_person"": ""Jane Smith"",
                    ""sponsor_code"": ""GSL002""
                },
                ""sponsor3"": {
                    ""sponsor_id"": 1003,
                    ""sponsor_name"": ""Future Systems Corp."",
                    ""office_phone"": ""555-0303"",
                    ""contact_person"": ""Alice Johnson"",
                    ""sponsor_code"": ""FSC003""
                },
                ""sponsor4"": {
                    ""sponsor_id"": 1004,
                    ""sponsor_name"": ""Innovative Minds Group"",
                    ""office_phone"": ""555-0404"",
                    ""contact_person"": ""Bob Wilson"",
                    ""sponsor_code"": ""IMG004""
                },
                ""sponsor5"": {
                    ""sponsor_id"": 1005,
                    ""sponsor_name"": ""NextGen Enterprises"",
                    ""office_phone"": ""555-0505"",
                    ""contact_person"": ""Carol Brown"",
                    ""sponsor_code"": ""NGE005""
                },
                ""sponsor6"": {
                    ""sponsor_id"": 1006,
                    ""sponsor_name"": ""Digital Dynamics Co."",
                    ""office_phone"": ""555-0606"",
                    ""contact_person"": ""David Lee"",
                    ""sponsor_code"": ""DDC006""
                },
                ""sponsor7"": {
                    ""sponsor_id"": 1007,
                    ""sponsor_name"": ""Quantum Quests Inc."",
                    ""office_phone"": ""555-0707"",
                    ""contact_person"": ""Emma Davis"",
                    ""sponsor_code"": ""QQI007""
                },
                ""sponsor8"": {
                    ""sponsor_id"": 1008,
                    ""sponsor_name"": ""Synergy Systems LLC"",
                    ""office_phone"": ""555-0808"",
                    ""contact_person"": ""Frank Miller"",
                    ""sponsor_code"": ""SSL008""
                },
                ""sponsor9"": {
                    ""sponsor_id"": 1009,
                    ""sponsor_name"": ""Apex Automation Corp."",
                    ""office_phone"": ""555-0909"",
                    ""contact_person"": ""Grace Taylor"",
                    ""sponsor_code"": ""AAC009""
                },
                ""sponsor10"": {
                    ""sponsor_id"": 1010,
                    ""sponsor_name"": ""Cyber Solutions Group"",
                    ""office_phone"": ""555-1010"",
                    ""contact_person"": ""Henry White"",
                    ""sponsor_code"": ""CSG010""
                },
                ""sponsor11"": {
                    ""sponsor_id"": 1011,
                    ""sponsor_name"": ""Tech Titans Ltd."",
                    ""office_phone"": ""555-1111"",
                    ""contact_person"": ""Ivy Green"",
                    ""sponsor_code"": ""TTL011""
                },
                ""sponsor12"": {
                    ""sponsor_id"": 1012,
                    ""sponsor_name"": ""Innovate Now Inc."",
                    ""office_phone"": ""555-1212"",
                    ""contact_person"": ""Jack Black"",
                    ""sponsor_code"": ""INI012""
                }
            }";
            initializeData(_mockJsonData);
        }

        public List<SponsorListItem> initializeData(string _mockJsonData)
        {
            return sponsorAllLists = dataCovertIntoList(_mockJsonData);
        }

        private List<SponsorListItem> dataCovertIntoList(string content)
        {
            List<SponsorListItem> result = new List<SponsorListItem>();
            try
            {
                var jsonDocument = JsonDocument.Parse(content);
                var root = jsonDocument.RootElement;
                foreach (var property in root.EnumerateObject())
                {
                    var sponsor = property.Value;
                    SponsorListItem sponsorItem = new SponsorListItem
                    {
                        SponsorId = sponsor.GetProperty("sponsor_id").GetInt64(),
                        SponsorName = sponsor.GetProperty("sponsor_name").GetString(),
                        SponsorPhone = sponsor.GetProperty("office_phone").GetString(),
                        ContactPerson = sponsor.GetProperty("contact_person").GetString(),
                        SponsorCode = sponsor.GetProperty("sponsor_code").GetString()
                    };
                    Console.WriteLine($"Testing: {sponsorItem.SponsorId}");
                    result.Add(sponsorItem);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
            return result;
        }

        
        public Object getSearchSponsor(string searchName, string searchCode, string searchTel)
        {
            searchName = searchName.Trim();
            searchCode = searchCode.Trim();
            searchTel = searchTel.Trim();

            Console.WriteLine($"Search parameters: Name: {searchName}, Code: {searchCode}, Tel: {searchTel}");

            searchResults = new List<SponsorListItem>();
            searchResults = sponsorAllLists;

            searchResults = searchResults.AsParallel().WithDegreeOfParallelism(3).Where(sponsor =>
                (searchCode.Equals("NULL") || sponsor.SponsorCode.IndexOf(searchCode, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (searchName.Equals("NULL") || sponsor.SponsorName.IndexOf(searchName, StringComparison.OrdinalIgnoreCase) >= 0) &&
                (searchTel.Equals("NULL") || sponsor.SponsorPhone.Contains(searchTel))
            ).ToList(); //Create 3 thread running parallel for the search O((N * (code + name + phone)) / 3)

            if (searchResults.Count == 0)
            {
                return "Not Found";
            }
            return searchResults;
        }

     
        public Object getSearchSponsorGeneral(string searchGeneral)
        {

            searchGeneral = searchGeneral.Trim();

            Console.WriteLine($"Search parameters: General: {searchGeneral} ");

            searchResults = new List<SponsorListItem>();
            searchResults = sponsorAllLists;
            bool isNumber = int.TryParse(searchGeneral, out _);

            if (!searchGeneral.Equals("NULL") && isNumber)
            {
                searchResults = searchResults.Where(sponsor =>
                   sponsor.SponsorPhone.Contains(searchGeneral)
               ).ToList();
            }
            else if (!searchGeneral.Equals("NULL"))
            {
                searchResults = searchResults.AsParallel().WithDegreeOfParallelism(3).Where(sponsor =>
                   sponsor.SponsorCode.IndexOf(searchGeneral, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   sponsor.SponsorName.IndexOf(searchGeneral, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   sponsor.ContactPerson.IndexOf(searchGeneral, StringComparison.OrdinalIgnoreCase) >= 0
               ).ToList();
            }

            if (searchResults.Count == 0)
            {
                return "Not Found";
            }
            return searchResults;
        }


        [Fact]
        public void Test1_InitializeUsers_NumberOfTotalSponsorLoad()
        {

            // Xunit.Assert
            var sponsors = sponsorAllLists as List<SponsorListItem>;
            Xunit.Assert.Equal(12, sponsors.Count);
        }

        [Theory]
        [InlineData("Tech", "NULL", "NULL", 2)]
        [InlineData("NULL", "TII001", "NULL", 1)]
        [InlineData("NULL", "NULL", "555-0101", 1)]
        [InlineData("Global", "GSL002", "555-0202", 1)]
        [InlineData("Nonexistent", "NULL", "NULL", "Not Found")]
        public void Test2_GetSearchSponsor_ShouldReturnCorrectResults(string name, string code, string tel, object expectedResult)
        {
         
            var result = getSearchSponsor(name, code, tel);

            if (expectedResult is int expectedCount)
            {
                Xunit.Assert.IsType<List<SponsorListItem>>(result);
                Xunit.Assert.Equal(expectedCount, (result as List<SponsorListItem>).Count);
            }
            else
            {
                Xunit.Assert.Equal(expectedResult, result);
            }
        }

        [Theory]
        [InlineData("Tech", 2)]
        [InlineData("0202", 1)]
        [InlineData("TII001", 1)]
        [InlineData("John Doe", 1)]
        [InlineData("Nonexistent_noo", "Not Found")]
        [InlineData("NULL", 12)]
        public void Test3_GetSearchSponsorGeneral_ShouldReturnCorrectResults(string searchGeneral, object expectedResult)
        {
            var result = getSearchSponsorGeneral(searchGeneral);

            if (expectedResult is int expectedCount)
            {
                Assert.IsType<List<SponsorListItem>>(result);
                Assert.Equal(expectedCount, (result as List<SponsorListItem>).Count);
            }
            else
            {
                Assert.IsType<string>(result);
                Assert.Equal(expectedResult, result);
            }
        }

        [Fact]
        public void Test4_GetSearchSponsorGeneral_WithNULL_ShouldReturnAllSponsors()
        {
            var result = getSearchSponsorGeneral("NULL");
            Xunit.Assert.IsType<List<SponsorListItem>>(result);
            Xunit.Assert.Equal(12, (result as List<SponsorListItem>).Count);
        }

        [Fact]
        public void Test5_GetSearchSponsor_WithAllNULL_ShouldReturnAllSponsors()
        {
            var result = getSearchSponsor("NULL", "NULL", "NULL");
            Xunit.Assert.IsType<List<SponsorListItem>>(result);
            Xunit.Assert.Equal(12, (result as List<SponsorListItem>).Count);
        }

        [Fact]
        public void Test6_GetSearchSponsor_CaseInsensitive_ShouldReturnCorrectResults()
        {
            var result = getSearchSponsor("tech", "NULL", "NULL");
            Xunit.Assert.IsType<List<SponsorListItem>>(result);
            Xunit.Assert.Equal(2, (result as List<SponsorListItem>).Count);
        }

        [Fact]
        public void Test7_GetSearchSponsorGeneral_PartialMatch_ShouldReturnCorrectResults()
        {
            var result = getSearchSponsorGeneral("Inc");
            Xunit.Assert.IsType<List<SponsorListItem>>(result);
            Xunit.Assert.Equal(3, (result as List<SponsorListItem>).Count);
        }

        [Fact]
        public void Test8_GetSearchSponsor_MultipleMatches_ShouldReturnCorrectResults()
        {
            var result = getSearchSponsor("NULL", "NULL", "555-0");
            Xunit.Assert.IsType<List<SponsorListItem>>(result);
            Xunit.Assert.Equal(9, (result as List<SponsorListItem>).Count);
        }

        [Fact]
        public void Test9_GetSearchSponsorGeneral_ContactPerson_ShouldReturnCorrectResults()
        {
            var result = getSearchSponsorGeneral("Alice");
            Xunit.Assert.IsType<List<SponsorListItem>>(result);
            Xunit.Assert.Equal(1, (result as List<SponsorListItem>).Count);
            Xunit.Assert.Equal("Alice Johnson", (result as List<SponsorListItem>)[0].ContactPerson);
        }

        [Fact]
        public void Test10_GetSearchSponsor_ExactMatch_ShouldReturnSingleResult()
        {
            var result = getSearchSponsor("Tech Innovations Inc.", "NULL", "NULL");
            Xunit.Assert.IsType<List<SponsorListItem>>(result);
            Xunit.Assert.Single(result as List<SponsorListItem>);
            Xunit.Assert.Equal("Tech Innovations Inc.", (result as List<SponsorListItem>)[0].SponsorName);
        }

        [Fact]
        public void Test11_GetSearchSponsorGeneral_NumericInput_ShouldSearchPhoneOnly()
        {
            var result = getSearchSponsorGeneral("0101");
            Xunit.Assert.IsType<List<SponsorListItem>>(result);
            Xunit.Assert.Single(result as List<SponsorListItem>);
            Xunit.Assert.Equal("555-0101", (result as List<SponsorListItem>)[0].SponsorPhone);
        }

        [Fact]
        public void Test12_GetSearchSponsor_TrimmedInput_ShouldReturnCorrectResults()
        {
            var result = getSearchSponsor("  Tech  ", "  TII001  ", "  555-0101  ");
            Xunit.Assert.IsType<List<SponsorListItem>>(result);
            Xunit.Assert.Single(result as List<SponsorListItem>);
            Xunit.Assert.Equal("Tech Innovations Inc.", (result as List<SponsorListItem>)[0].SponsorName);
        }
    }
}