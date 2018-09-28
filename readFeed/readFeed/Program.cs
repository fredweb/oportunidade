using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using readFeed;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace oportunidade
{
    internal class Program
    {
        private static readonly string[] preposicoes = new[]
        {
            " A ",
            " O ",
            " E ",
            " É ",
            " Ante ",
            " Quais ",
            " Quando ",
             " ao ",
             " aos ",
             " aonde ",
              " aonde ",
            " Mais ",
            " Pois ",
            " Após ",
            " Até ",
            " Com ",
            " Contra ",
            " De ",
            " Do ",
            " Da ",
            " Desde ",
            " Em ",
            " Entre ",
            " Para ",
            " Per ",
            " Perante ",
            " Por ",
            " Sem ",
            " Sob ",
            " Sobre ",
             " Trás ",
             " Afora ",
             " Conforme ",
             " Consoante ",
             " Exceto ",
             " Salvo ",
             " Malgrado ",
             " Malgrado "
        };

        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            Task<Analise> task = GetFeed();
            task.Wait();
            Console.WriteLine(string.Join(" - ", task.Result.PrincipaisPalavras));
            foreach (var item in task.Result.Topicos)
            {
                Console.WriteLine(item.Titulo + " - " + item.QuantidadePalavas);
            }
            Console.ReadLine();
        }

        private static async Task<Analise> GetFeed()
        {
            var retorno = new Analise();
            var strBuild = new StringBuilder();
            var noticias = await GetItemFeed();
            if (noticias.Any())
            {
                foreach (var noticia in noticias.OrderBy(o => o.Published))
                {
                    var split = noticia.Description.Split("<p>");
                    var strAux = string.Empty;
                    if (split[0] != null && split[1] != null)
                    {
                        if (!string.IsNullOrWhiteSpace(split[0]))
                        {
                            strAux = split[0];
                        }
                        else
                        {
                            strAux = split[1];
                        }
                    }

                    foreach (var proposicao in preposicoes)
                    {
                        if (strAux.ToUpper().Contains(proposicao.ToUpper()))
                            strAux = strAux.ToUpper().Replace(proposicao.ToUpper(), " ");
                    }

                    var cont = strAux.Split(" ");
                    if (cont.Any())
                    {
                        retorno.Topicos.Add(new Topicos
                        {
                            Titulo = noticia.Title,
                            QuantidadePalavas = cont.Count(w => w.Length > 3)
                        });

                        strBuild.Append(strAux);
                    }
                }
                retorno.PrincipaisPalavras = CatalogarPalavras(strBuild.ToString());
            }
            return retorno;
        }

        private static List<string> CatalogarPalavras(string texto)
        {
            var retorno = new List<string>();
            var analise = new List<Topicos>();
            var topico = texto.Split(" ");
            foreach (var palavra in topico.Where(w => w.Length > 3).Distinct())
            {
                analise.Add(new Topicos
                {
                    QuantidadePalavas = topico.Count(w => w.Equals(palavra)),
                    Titulo = palavra
                });
            }
            return analise.OrderByDescending(o => o.QuantidadePalavas).Take(10).Select(s => s.Titulo).ToList();
        }

        private static async Task<List<SyndicationItem>> GetItemFeed()
        {
            var ItemFeed = new List<SyndicationItem>();
            using (var http = new HttpClient())
            {
                var strFeed = await http.GetStringAsync("http://www.minutoseguros.com.br/blog/feed/");
                if (!string.IsNullOrWhiteSpace(strFeed))
                {
                    using (var xmlReader = XmlReader.Create(new StringReader(strFeed)))
                    {
                        xmlReader.Read();
                        var feedReader = new RssFeedReader(xmlReader);
                        while (await feedReader.Read())
                        {
                            switch (feedReader.ElementType)
                            {
                                case SyndicationElementType.Item:
                                    ISyndicationItem item = await feedReader.ReadItem();
                                    ItemFeed.Add((SyndicationItem)item);
                                    break;
                            }
                        }
                    }
                }
            }
            return ItemFeed;
        }
    }
}