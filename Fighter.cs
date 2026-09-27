using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace JogoLutaCSharp
{
    /// <summary>
    /// Representa um personagem lutador: posicao, fisica (pulo/gravidade),
    /// vida e o soco (ataque). Equivalente a classe "Lutador" da versao em Python.
    /// </summary>
    public class Fighter
    {
        public Rectangle Rect;
        public Image Sprite;

        // direcao fixa: para qual lado este personagem solta o soco
        // (nao muda quando ele anda para tras — igual na versao Python)
        public bool OlhandoDireita;

        public int Vida = 100;

        // fisica simples (pulo)
        public float VelY;
        public bool NoChao = true;

        // combate
        public bool Socando;
        public bool AtingiuNesteSoco;
        private long _horaFimSoco;
        private long _horaLiberadoParaSocar;

        // teclas deste jogador
        public readonly Keys TeclaEsquerda;
        public readonly Keys TeclaDireita;
        public readonly Keys TeclaPular;
        public readonly Keys TeclaSocar;

        public Fighter(int x, int chaoY, Image sprite, bool olhandoDireita,
                        Keys esquerda, Keys direita, Keys pular, Keys socar)
        {
            Sprite = sprite;
            int largura = sprite.Width;
            int altura = sprite.Height;
            Rect = new Rectangle(x, chaoY - altura, largura, altura);

            OlhandoDireita = olhandoDireita;
            TeclaEsquerda = esquerda;
            TeclaDireita = direita;
            TeclaPular = pular;
            TeclaSocar = socar;
        }

        /// <summary>Retorna o retangulo (hitbox) do soco, na direcao fixa deste lutador.</summary>
        public Rectangle HitboxSoco(int alcance)
        {
            int x = OlhandoDireita ? Rect.Right : Rect.Left - alcance;
            int y = Rect.Top + Rect.Height / 2 - 10;
            return new Rectangle(x, y, alcance, 20);
        }

        /// <summary>Inicia um soco, se o cooldown ja tiver passado.</summary>
        public void TentarSocar(long agoraMs, int duracaoMs, int cooldownMs)
        {
            if (agoraMs < _horaLiberadoParaSocar) return;

            Socando = true;
            AtingiuNesteSoco = false;
            _horaFimSoco = agoraMs + duracaoMs;
            _horaLiberadoParaSocar = agoraMs + cooldownMs;
        }

        public void Atualizar(HashSet<Keys> teclas, long agoraMs,
                               int limiteEsquerdo, int limiteDireito, int chaoY,
                               float gravidade, float forcaPulo, int velocidadeMovimento)
        {
            // --- movimento horizontal ---
            if (teclas.Contains(TeclaEsquerda)) Rect.X -= velocidadeMovimento;
            if (teclas.Contains(TeclaDireita)) Rect.X += velocidadeMovimento;

            // nao deixa sair da area do jogador (metade da tela)
            Rect.X = Math.Max(limiteEsquerdo, Math.Min(limiteDireito - Rect.Width, Rect.X));

            // --- pulo (gravidade simples) ---
            if (teclas.Contains(TeclaPular) && NoChao)
            {
                VelY = forcaPulo;
                NoChao = false;
            }

            VelY += gravidade;
            Rect.Y += (int)VelY;

            if (Rect.Bottom >= chaoY)
            {
                Rect.Y = chaoY - Rect.Height;
                VelY = 0;
                NoChao = true;
            }

            // --- soco ---
            if (Socando && agoraMs >= _horaFimSoco)
            {
                Socando = false;
            }
        }
    }
}
