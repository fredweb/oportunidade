using System.Collections.Generic;

namespace readFeed
{
    public class Analise
    {
        public Analise()
        {
            PrincipaisPalavras = new List<string>();
            Topicos = new List<Topicos>();
        }
        public List<string> PrincipaisPalavras { get; set; }
        public List<Topicos> Topicos { get; set; }
    }

    public class Topicos
    {
        public string Titulo { get; set; }
        public int QuantidadePalavas { get; set; }
    }
}
