using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MarvelWebAPI.Models
{
    public class CharactersModel
    {
        public class MarvelCharacterData
        {
            public int Code { get; set; }
            public string Status { get; set; }
            public string Copyright { get; set; }
            public string AttributionText { get; set; }
            public string Etag { get; set; }
            public DataContainer Data { get; set; }
        }

        public class DataContainer
        {
            public int Offset { get; set; }
            public int Limit { get; set; }
            public int Total { get; set; }
            public int Count { get; set; }
            public List<Character> Results { get; set; }
        }

        public class Character
        {
            [Key]
            public int Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Path { get; set; }
            public string Extension { get; set; }
        }

        public class Thumbnail
        {
            public string Path { get; set; }
            public string Extension { get; set; }
        }



    }
}
