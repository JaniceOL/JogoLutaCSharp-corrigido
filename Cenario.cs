using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;

namespace JogoLutaCSharp
{
    /// <summary>
    /// Monta a imagem de fundo da arena, uma unica vez no inicio do jogo.
    /// Se existir na pasta Sprites um PNG cujo nome comece com "cenario" (ex: cenario.png,
    /// "cenario kof.png"), usa essa imagem; senao desenha um cenario por codigo.
    /// </summary>
    internal static class Cenario
    {
        public static Image Criar(int largura, int altura, int chaoY)
        {
            string pastaSprites = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Sprites");
            string caminho = Directory.Exists(pastaSprites)
                ? Directory.GetFiles(pastaSprites, "cenario*.png").OrderBy(f => f).FirstOrDefault()
                : null;

            if (caminho != null)
            {
                using var original = Image.FromFile(caminho);
                return Esticar(original, largura, altura);
            }

            return Desenhar(largura, altura, chaoY);
        }

        // Redimensiona a imagem para o tamanho exato da tela
        private static Image Esticar(Image original, int largura, int altura)
        {
            var bmp = new Bitmap(largura, altura);
            using var g = Graphics.FromImage(bmp);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(original, 0, 0, largura, altura);
            return bmp;
        }

        // Cenario desenhado por codigo: ceu de por do sol, sol, montanhas e chao de pedra
        private static Image Desenhar(int largura, int altura, int chaoY)
        {
            var bmp = new Bitmap(largura, altura);
            using var g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. Ceu em degrade (roxo em cima, laranja no horizonte)
            var areaCeu = new Rectangle(0, 0, largura, chaoY);
            using (var ceu = new LinearGradientBrush(areaCeu,
                       Color.FromArgb(40, 30, 90), Color.FromArgb(250, 150, 80), LinearGradientMode.Vertical))
                g.FillRectangle(ceu, areaCeu);

            // 2. Sol
            using (var sol = new SolidBrush(Color.FromArgb(255, 220, 120)))
                g.FillEllipse(sol, largura / 2 - 60, chaoY - 190, 120, 120);

            // 3. Montanhas: a camada de tras e mais alta e mais clara
            DesenharMontanhas(g, largura, chaoY, 170, Color.FromArgb(130, 75, 115), 0);
            DesenharMontanhas(g, largura, chaoY, 100, Color.FromArgb(75, 40, 80), 70);

            // 4. Chao de pedra
            var areaChao = new Rectangle(0, chaoY, largura, altura - chaoY);
            using (var chao = new LinearGradientBrush(areaChao,
                       Color.FromArgb(110, 95, 90), Color.FromArgb(60, 50, 50), LinearGradientMode.Vertical))
                g.FillRectangle(chao, areaChao);

            using (var juntas = new Pen(Color.FromArgb(45, 35, 35), 2))
            {
                g.DrawLine(juntas, 0, chaoY + 25, largura, chaoY + 25);
                for (int x = 0; x <= largura; x += 80)
                {
                    g.DrawLine(juntas, x, chaoY, x, chaoY + 25);
                    g.DrawLine(juntas, x + 40, chaoY + 25, x + 40, altura);
                }
            }

            using (var linhaChao = new Pen(Color.FromArgb(20, 20, 20), 3))
                g.DrawLine(linhaChao, 0, chaoY, largura, chaoY);

            return bmp;
        }

        // Faz uma fileira de picos em zigue-zague, alternando picos altos e baixos
        private static void DesenharMontanhas(Graphics g, int largura, int baseY, int alturaMax, Color cor, int deslocamento)
        {
            var pontos = new List<Point> { new Point(0, baseY) };
            bool picoAlto = true;

            for (int x = -deslocamento; x <= largura + 160; x += 160)
            {
                int alturaPico = picoAlto ? alturaMax : alturaMax * 2 / 3;
                pontos.Add(new Point(x + 80, baseY - alturaPico));
                pontos.Add(new Point(x + 160, baseY - alturaMax / 4));
                picoAlto = !picoAlto;
            }

            pontos.Add(new Point(largura, baseY));

            using var pincel = new SolidBrush(cor);
            g.FillPolygon(pincel, pontos.ToArray());
        }
    }
}
