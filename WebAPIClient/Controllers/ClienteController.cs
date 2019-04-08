using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using WebAPIClient.Models;
using Newtonsoft.Json.Linq;

namespace WebAPIClient.Controllers
{
    [EnableCors(origins: "*", headers: " * ", methods: "*")]
    public class ClienteController : ApiController
    {

        private static List<Cliente> clientes;
        private static List<Endereco> enderecos;
        [HttpPost]
        public HttpResponseMessage PostClient(JObject cliente)
        {
            var novoCliente = cliente.ToObject<Cliente>();
            var novosEnderecos = cliente["Enderecos"].ToObject<List<Endereco>>();

            try
            {
                if (ModelState.IsValid)
                {
                    int maxId = 0;
                    int maxEndId = 0;
                    if (clientes == null || clientes.Count() == 0)
                    {
                        clientes = new List<Cliente>();
                        enderecos = new List<Endereco>();
                        maxId = 1;
                        maxEndId = 1;
                    }
                    else
                    {
                        maxEndId = enderecos.Max(c => c.Id) + 1;
                        maxId = clientes.Max(c => c.Id) + 1;                       
                    }
                    novoCliente.Id = maxId;
                    var clienteBanco = (from c in clientes where c.CPF_CNPJ == novoCliente.CPF_CNPJ && c.TipoCliente == novoCliente.TipoCliente select c).FirstOrDefault();

                    if (clienteBanco == null)
                    {
                        clientes.Add(novoCliente);
                        
                        foreach (var en in novosEnderecos) {
                            if (en.CEP != "")
                            {
                                enderecos.Add(new Endereco()
                                {
                                    IdCliente = novoCliente.Id,
                                    CEP = en.CEP,
                                    Cidade = en.Cidade,
                                    Complemento = en.Complemento,
                                    Id = maxEndId,
                                    Logradouro = en.Logradouro,
                                    Bairro=en.Bairro,
                                    Numero = en.Numero,
                                    UF = en.UF
                                });
                                maxEndId = maxEndId + 1;
                            }
                        }
                        HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, cliente);
                        return response;
                    }
                    else
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Cliente já existe");
                    }
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Formulário não foi preenchido corretamente");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Formulário não foi preenchido corretamente", ex);
            }
        }

        // Deletar Cliente  
        [ResponseType(typeof(Cliente))]
        public HttpResponseMessage Delete(int id)
        {
            try { 
            var cliente = (from c in clientes where c.Id == id select c).FirstOrDefault();
            if (cliente == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            clientes.Remove(cliente);
            return Request.CreateResponse(HttpStatusCode.OK, cliente);

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound,"Erro ao Excluir o Cliente",ex);
            }

        }

        //Editar Cliente
        [ResponseType(typeof(void))]
        public HttpResponseMessage Put(string id, JObject cliente)
        {

            var clienteEditado = cliente.ToObject<Cliente>();
            var enderecosEditados = cliente["Enderecos"].ToObject<List<Endereco>>();
            var idConvertido=int.Parse(id);
            try
            {
                if (!ModelState.IsValid)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                }

                var clienteBanco = (from c in clientes where c.Id == idConvertido select c).FirstOrDefault();

                var clienteOutro = (from c in clientes where c.CPF_CNPJ==clienteEditado.CPF_CNPJ && c.Id!=idConvertido && c.TipoCliente==clienteEditado.TipoCliente select c).FirstOrDefault();

                if (clienteOutro == null)
                {
                    clientes.Remove(clienteBanco);

                    clientes.Add(clienteEditado);

                    var enderecosBanco = (from c in enderecos where c.IdCliente == idConvertido select c).ToList();
                    if (enderecos != null && enderecos.Count() > 0) {
                        foreach (var end in enderecosBanco) {
                            enderecos.Remove(end);
                        }
                    }
                    int max = enderecos!=null && enderecos.Count()>0? enderecos.Max(x => x.Id): 0;
                    foreach (var end in enderecosEditados) {
                        end.IdCliente = idConvertido;
                        end.Id = max + 1;
                        max++;
                    }
                    enderecos.AddRange(enderecosEditados);

                    return Request.CreateResponse(HttpStatusCode.OK, cliente);
                }else {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Cliente já existe!");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, "Erro ao Editar o Cliente", ex);
            }

            

        }

        //Retorna todos os Clientes Cadastrados
        [HttpGet]
        public IEnumerable<Cliente> GetAllClients()
        {
            return clientes!=null?clientes.OrderBy(x=>x.Nome).ToList() : null;
        }

        [HttpGet]
        public IHttpActionResult GetClient(int id)
        {
            var cliente = clientes.FirstOrDefault(c => c.Id == id);
            cliente.Enderecos = enderecos.Where(x=>x.IdCliente==cliente.Id).ToList();
            JObject jObj = JObject.FromObject(cliente);
            if (cliente == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(jObj);
            }
        }
    }
}
