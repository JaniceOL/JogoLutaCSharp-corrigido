using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace JogoLutaCSharp
{
    public class MainForm : Form
    {
        // ------------------------------------------------------------------
        // CONFIGURACOES GERAIS (mesmos valores da versao em Python)
        // ------------------------------------------------------------------
        private const int LarguraTela = 800;
        private const int AlturaTela = 450;
        private const int ChaoY = AlturaTela - 100;

        private const float Gravidade = 0.9f;
        private const float ForcaPulo = -15f;
        private const int VelocidadeMovimento = 5;

        private const int DanoSoco = 8;
        private const int AlcanceSoco = 60;
        private const int DuracaoSocoMs = 150;
        private const int CooldownSocoMs = 450;

        private const int VidaMaxima = 100;
        private const int AlturaSprite = 120;

        // ------------------------------------------------------------------
        private readonly System.Windows.Forms.Timer _timer;
        private readonly HashSet<Keys> _teclasPressionadas = new();
        private readonly Stopwatch _relogio = Stopwatch.StartNew();

        private Image _cenario;
        private Image _sprite1;
        private Image _sprite2;
        private Fighter _jogador1;
        private Fighter _jogador2;

        private bool _jogoAcabou;
        private string _textoVencedor = "";

        public MainForm()
        {
            Text = "Jogo de Luta Minimo - C#";
            ClientSize = new Size(LarguraTela, AlturaTela);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            DoubleBuffered = true;   // evita o "piscar" da tela ao redesenhar
            KeyPreview = true;       // permite o Form capturar as teclas

            _cenario = Cenario.Criar(LarguraTela, AlturaTela, ChaoY);
            CarregarSprites();
            NovaPartida();

            KeyDown += MainForm_KeyDown;
            KeyUp += MainForm_KeyUp;

            // "game loop": um Timer chamando Atualizar + Redesenhar ~60x por segundo
            _timer = new System.Windows.Forms.Timer { Interval = 16 };
            _timer.Tick += (s, e) =>
            {
                AtualizarJogo();
                Invalidate(); // forca o OnPaint a rodar de novo
            };
            _timer.Start();
        }

        // --------------------------------------------------------------
        // CARREGAMENTO DOS SPRITES
        // --------------------------------------------------------------
        private void CarregarSprites()
        {
            string pastaBase = AppDomain.CurrentDomain.BaseDirectory;
            string caminho1 = Path.Combine(pastaBase, "Sprites", "sprite_p1.png");
            string caminho2 = Path.Combine(pastaBase, "Sprites", "sprite_p2.png");

            using var original1 = Image.FromFile(caminho1);
            using var original2 = Image.FromFile(caminho2);

            // P1 fica a esquerda olhando para a direita. O sprite_p1.png vem virado
            // para a esquerda, entao espelhamos (se trocar a imagem por uma virada
            // para a direita, mude para espelhar: false)
            _sprite1 = Redimensionar(original1, AlturaSprite, espelhar: true);
            // P2 fica a direita olhando para a esquerda. O sprite_p2.png ja vem virado
            // para a esquerda, entao NAO espelhamos (se trocar a imagem por uma virada
            // para a direita, mude para espelhar: true)
            _sprite2 = Redimensionar(original2, AlturaSprite, espelhar: false);
        }

        private static Image Redimensionar(Image original, int alturaAlvo, bool espelhar)
        {
            float proporcao = (float)alturaAlvo / original.Height;
            int larguraAlvo = (int)(original.Width * proporcao);

            var bmp = new Bitmap(larguraAlvo, alturaAlvo);
            using (var g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(original, 0, 0, larguraAlvo, alturaAlvo);
            }

            if (espelhar)
            {
                bmp.RotateFlip(RotateFlipType.RotateNoneFlipX);
            }
            return bmp;
        }

        // --------------------------------------------------------------
        // CONTROLE DA PARTIDA
        // --------------------------------------------------------------
        private void NovaPartida()
        {
            _jogador1 = new Fighter(150, ChaoY, _sprite1, olhandoDireita: true,
                esquerda: Keys.A, direita: Keys.D, pular: Keys.W, socar: Keys.Space);

            _jogador2 = new Fighter(600, ChaoY, _sprite2, olhandoDireita: false,
                esquerda: Keys.Left, direita: Keys.Right, pular: Keys.Up, socar: Keys.Enter);

            _jogoAcabou = false;
            _textoVencedor = "";
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            _teclasPressionadas.Add(e.KeyCode);

            long agora = _relogio.ElapsedMilliseconds;

            if (!_jogoAcabou)
            {
                if (e.KeyCode == _jogador1.TeclaSocar)
                    _jogador1.TentarSocar(agora, DuracaoSocoMs, CooldownSocoMs);
                if (e.KeyCode == _jogador2.TeclaSocar)
                    _jogador2.TentarSocar(agora, DuracaoSocoMs, CooldownSocoMs);
            }
            else if (e.KeyCode == Keys.R)
            {
                NovaPartida();
            }

            if (e.KeyCode == Keys.Escape) Close();
        }

        private void MainForm_KeyUp(object sender, KeyEventArgs e)
        {
            _teclasPressionadas.Remove(e.KeyCode);
        }

        // --------------------------------------------------------------
        // ATUALIZACAO (fisica, movimento, combate)
        // --------------------------------------------------------------
        private void AtualizarJogo()
        {
            if (_jogoAcabou) return;

            long agora = _relogio.ElapsedMilliseconds;

            _jogador1.Atualizar(_teclasPressionadas, agora, 0, LarguraTela / 2,
                ChaoY, Gravidade, ForcaPulo, VelocidadeMovimento);
            _jogador2.Atualizar(_teclasPressionadas, agora, LarguraTela / 2, LarguraTela,
                ChaoY, Gravidade, ForcaPulo, VelocidadeMovimento);

            ResolverCombate(_jogador1, _jogador2);
            ResolverCombate(_jogador2, _jogador1);

            if (_jogador1.Vida <= 0 || _jogador2.Vida <= 0)
            {
                _jogoAcabou = true;
                if (_jogador1.Vida <= 0 && _jogador2.Vida <= 0)
                    _textoVencedor = "EMPATE!";
                else if (_jogador1.Vida <= 0)
                    _textoVencedor = "JOGADOR 2 VENCEU!";
                else
                    _textoVencedor = "JOGADOR 1 VENCEU!";
            }
        }

        /// <summary>Se o atacante estiver socando e acertar o defensor, aplica dano uma unica vez.</summary>
        private static void ResolverCombate(Fighter atacante, Fighter defensor)
        {
            if (!atacante.Socando || atacante.AtingiuNesteSoco) return;

            if (atacante.HitboxSoco(AlcanceSoco).IntersectsWith(defensor.Rect))
            {
                defensor.Vida = Math.Max(0, defensor.Vida - DanoSoco);
                atacante.AtingiuNesteSoco = true;
            }
        }

        // --------------------------------------------------------------
        // DESENHO
        // --------------------------------------------------------------
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // O cenario e desenhado primeiro, para ficar ATRAS de todo o resto
            g.DrawImage(_cenario, 0, 0, LarguraTela, AlturaTela);

            DesenharLutador(g, _jogador1);
            DesenharLutador(g, _jogador2);

            DesenharBarraVida(g, 20, _jogador1.Vida, viradaEsquerda: false);
            DesenharBarraVida(g, LarguraTela - 320, _jogador2.Vida, viradaEsquerda: true);

            using (var fonteRotulo = new Font("Segoe UI", 11, FontStyle.Bold))
            {
                // cores claras para dar leitura sobre o ceu escuro do cenario
                g.DrawString("P1", fonteRotulo, Brushes.LightSkyBlue, 20, 45);
                g.DrawString("P2", fonteRotulo, Brushes.LightCoral, LarguraTela - 340, 45);
            }

            if (_jogoAcabou)
            {
                using var fonteGrande = new Font("Segoe UI", 24, FontStyle.Bold);
                using var fontePequena = new Font("Segoe UI", 12);

                // faixa escura semitransparente para o texto aparecer sobre o cenario
                using (var faixa = new SolidBrush(Color.FromArgb(170, 0, 0, 0)))
                    g.FillRectangle(faixa, 0, AlturaTela / 2 - 45, LarguraTela, 90);

                SizeF tamanho = g.MeasureString(_textoVencedor, fonteGrande);
                g.DrawString(_textoVencedor, fonteGrande, Brushes.White,
                    LarguraTela / 2f - tamanho.Width / 2f, AlturaTela / 2f - 30f);

                const string textoR = "Pressione R para jogar novamente";
                SizeF tamanhoR = g.MeasureString(textoR, fontePequena);
                g.DrawString(textoR, fontePequena, Brushes.White,
                    LarguraTela / 2f - tamanhoR.Width / 2f, AlturaTela / 2f + 10f);
            }
        }

        private static void DesenharLutador(Graphics g, Fighter f)
        {
            g.DrawImage(f.Sprite, f.Rect);

            // contorno do alcance do soco (so visual, ajuda a estudar a hitbox)
            if (f.Socando)
            {
                using var caneta = new Pen(Color.White, 2);
                g.DrawRectangle(caneta, f.HitboxSoco(AlcanceSoco));
            }
        }

        private static void DesenharBarraVida(Graphics g, int x, int vida, bool viradaEsquerda)
        {
            const int largura = 300;
            const int altura = 22;
            float proporcao = vida / (float)VidaMaxima;

            using (var fundo = new SolidBrush(Color.FromArgb(90, 90, 90)))
                g.FillRectangle(fundo, x, 20, largura, altura);

            using (var verde = new SolidBrush(Color.FromArgb(60, 200, 90)))
            {
                if (viradaEsquerda)
                    g.FillRectangle(verde, x + largura * (1 - proporcao), 20, largura * proporcao, altura);
                else
                    g.FillRectangle(verde, x, 20, largura * proporcao, altura);
            }

            using (var borda = new Pen(Color.Black, 3))
                g.DrawRectangle(borda, x, 20, largura, altura);
        }
    }
}
