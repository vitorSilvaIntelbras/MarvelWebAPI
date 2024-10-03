using MarvelWebAPI.Context;
using MarvelWebAPI.Autentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static MarvelWebAPI.Models.CharactersModel;
namespace MarvelWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarvelController : Controller
    {
        private readonly string _publicKey;
        private readonly string _privateKey;
        private readonly MarvelContext _context;

        public MarvelController(MarvelContext context)
        {
            _publicKey = "29fdb57cfbece59e50a8f8fe13399f76";
            _privateKey = "90dcc58b7a8c2977a74074d97376b953bcdacaf6";
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> BuscarQuadrinhos()
        {
            var auth = new MarvelApiAuth(_publicKey, _privateKey);
            var authenticationString = auth.GetAuthenticationString();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                var url = $"http://gateway.marvel.com/v1/public/comics?{authenticationString}";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    // Processar o conteúdo da resposta
                    return Ok(responseBody);
                }
                else
                {
                    return BadRequest("Erro ao obter os quadrinhos");
                }

            }
        }

        [HttpGet("personagens")]
        public async Task<IActionResult> BuscarTodos()
        {
            var auth = new MarvelApiAuth(_publicKey, _privateKey);
            var authenticationString = auth.GetAuthenticationString();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                var url = $"http://gateway.marvel.com/v1/public/characters?{authenticationString}";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    // Processar o conteúdo da resposta
                    return Ok(responseBody);
                }
                else
                {
                    return BadRequest("Erro ao obter os quadrinhos");
                }

            }
        }

        [HttpGet("personagens/{id}")]
        public async Task<IActionResult> PesonagemPorId(int id)
        {
            var auth = new MarvelApiAuth(_publicKey, _privateKey);
            var authenticationString = auth.GetAuthenticationString();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                var url = $"http://gateway.marvel.com/v1/public/characters/{id}?{authenticationString}";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();

                    // Aqui desserializamos a resposta para o modelo de dados
                    var personagemData = JsonConvert.DeserializeObject<MarvelCharacterData>(responseBody);

                    // Verifica se há algum resultado
                    if (personagemData.Data.Results != null && personagemData.Data.Results.Count > 0)
                    {
                        var personagem = personagemData.Data.Results.First();
                        return Ok(personagem);  // Retorna os detalhes do personagem
                    }
                    else
                    {
                        return NotFound($"Personagem com ID: {id} não encontrado.");
                    }
                }
                else
                {
                    return BadRequest($"Erro ao obter o personagem com ID: {id}");
                }
            }
        }

        [HttpPost("salvar-personagens")]
        public async Task<IActionResult> SalvarPersonagens()
        {
            var auth = new MarvelApiAuth(_publicKey, _privateKey);
            var authenticationString = auth.GetAuthenticationString();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");

                var url = $"http://gateway.marvel.com/v1/public/characters?{authenticationString}";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var personagemData = JsonConvert.DeserializeObject<MarvelCharacterData>(responseBody);

                    // Verifica se há resultados
                    if (personagemData.Data.Results != null && personagemData.Data.Results.Any())
                    {
                        foreach (var personagem in personagemData.Data.Results)
                        {
                            // Busca o personagem no banco de dados pelo ID
                            var personagemExistente = await _context.Personagens.FindAsync(personagem.Id);

                            if (personagemExistente != null)
                            {
                                // Atualiza as propriedades do personagem existente com os dados da API
                                personagemExistente.Name = personagem.Name;
                                personagemExistente.Description = personagem.Description;
                                personagemExistente.Path = personagem.Path;
                                personagemExistente.Extension = personagem.Extension;

                                // Se precisar, você pode incluir outros atributos que deseja atualizar
                            }
                            else
                            {
                                // Se não existir, adiciona o personagem ao contexto
                                _context.Personagens.Add(personagem);
                            }
                        }

                        // Salva as alterações no banco de dados
                        await _context.SaveChangesAsync();
                        return Ok("Personagens salvos ou atualizados com sucesso!");
                    }
                    else
                    {
                        return NotFound("Nenhum personagem encontrado.");
                    }
                }
                else
                {
                    return BadRequest("Erro ao obter os personagens da API da Marvel.");
                }
            }
        }


        [HttpGet("buscar-personagem/{nome}")]
        public async Task<IActionResult> BuscarPersonagemPorNome(string nome)
        {
            var personagem = await _context.Personagens
            .FirstOrDefaultAsync(p => p.Name.Equals(nome));

            if (personagem != null)
            {
                return Ok(personagem);
            }
            else
            {
                return NotFound($"Personagem com o nome {nome} não encontrado.");
            }
        }

        [HttpPost("{nome}")]
        public async Task<IActionResult> SalvarCaracteristicasPersonagem(string nome, [FromBody] Character personagemRequest)
        {
            if (personagemRequest == null)
            {
                return BadRequest("Dados do personagem não podem ser nulos.");
            }

            // Busca o personagem pela API da Marvel usando o nome
            var auth = new MarvelApiAuth(_publicKey, _privateKey);
            var authenticationString = auth.GetAuthenticationString();

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                var url = $"http://gateway.marvel.com/v1/public/characters?name={Uri.EscapeDataString(nome)}&{authenticationString}";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var personagemData = JsonConvert.DeserializeObject<MarvelCharacterData>(responseBody);

                    // Verifica se há resultados e salva no banco
                    if (personagemData.Data.Results != null && personagemData.Data.Results.Any())
                    {
                        foreach (var personagem in personagemData.Data.Results)
                        {
                            // Atualiza as propriedades do personagem
                            personagem.Name = personagemRequest.Name;
                            personagem.Description = personagemRequest.Description;
                            personagem.Path = personagemRequest.Path;
                            personagem.Extension = personagemRequest.Extension;

                            // Verifica se o personagem já existe no banco
                            if (!_context.Personagens.Any(p => p.Id == personagem.Id))
                            {
                                // Adiciona o personagem ao contexto
                                _context.Personagens.Add(personagem);
                            }
                        }

                        // Salva as alterações no banco de dados
                        await _context.SaveChangesAsync();
                        return Ok("Características do personagem salvas com sucesso!");
                    }
                    else
                    {
                        return NotFound("Nenhum personagem encontrado com o nome fornecido.");
                    }
                }
                else
                {
                    return BadRequest("Erro ao obter o personagem da API da Marvel.");
                }
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarCaracteristicasPersonagem(int id, [FromBody] Character personagemRequest)
        {
            if (personagemRequest == null)
            {
                return BadRequest("Dados do personagem não podem ser nulos.");
            }

            // Busca o personagem no banco de dados pelo ID
            var personagemExistente = await _context.Personagens.FindAsync(id);

            if (personagemExistente == null)
            {
                return NotFound("Personagem não encontrado.");
            }

            // Atualiza as propriedades do personagem existente
            personagemExistente.Name = personagemRequest.Name;
            personagemExistente.Description = personagemRequest.Description;
            personagemExistente.Path = personagemRequest.Path;
            personagemExistente.Extension = personagemRequest.Extension;

            // Salva as alterações no banco de dados
            await _context.SaveChangesAsync();

            return Ok("Características do personagem atualizadas com sucesso!");
        }


        
    }

   


}
