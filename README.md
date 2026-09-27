# Jogo de Luta Mínimo — versão C# (Windows Forms)

Porta do `jogo_luta.py` para C#, mesma lógica (movimento, pulo, soco com
alcance/cooldown, vida, condição de vitória), usando **Windows Forms**
em vez de `pygame`.

## Requisitos

- Windows
- [.NET SDK 8.0](https://dotnet.microsoft.com/download) (ou ajuste o
  `TargetFramework` no `.csproj` para a versão que você tiver instalada,
  ex: `net6.0-windows`)

## Como rodar

Pelo terminal, dentro da pasta `JogoLutaCSharp`:

```
dotnet run
```

Ou abra `JogoLutaCSharp.csproj` no Visual Studio e aperte F5.

## Controles

Jogador 1 (esquerda):
- `A` / `D` — mover
- `W` — pular
- `ESPAÇO` — socar

Jogador 2 (direita):
- Setas esquerda/direita — mover
- Seta cima — pular
- `ENTER` — socar

`R` reinicia a partida (quando alguém vence). `ESC` fecha o jogo.

## Estrutura do projeto

```
JogoLutaCSharp/
├── JogoLutaCSharp.csproj   # arquivo de projeto (.NET, Windows Forms)
├── Program.cs              # ponto de entrada (Main)
├── MainForm.cs             # loop do jogo, teclado e desenho (equivalente ao main() do Python)
├── Fighter.cs              # classe do lutador (equivalente a Lutador em Python)
└── Sprites/
    ├── sprite_p1.png
    └── sprite_p2.png
```

## Como funciona (pra quem está estudando)

- **Game loop**: um `System.Windows.Forms.Timer` chama `AtualizarJogo()` e
  depois `Invalidate()` (que dispara `OnPaint`) a cada ~16ms, imitando o
  loop `while rodando` do pygame.
- **Entrada contínua** (mover, pular): guardamos as teclas atualmente
  pressionadas num `HashSet<Keys>`, atualizado em `KeyDown`/`KeyUp`.
- **Entrada de disparo único** (socar): tratada direto no evento `KeyDown`,
  com um cooldown (`TentarSocar`) que evita spam — mesmo se o Windows
  repetir o evento automaticamente ao segurar a tecla.
- **Colisão**: `Rectangle.IntersectsWith`, equivalente ao `colliderect`
  do pygame.
- **Desenho**: tudo acontece dentro de `OnPaint`, usando `Graphics`
  (`DrawImage`, `FillRectangle`, `DrawString`) — o equivalente ao
  `tela.blit()` / `pygame.draw.*` da versão Python.

## Observação

Este código não pôde ser compilado neste ambiente (sandbox Linux, sem o
SDK do .NET e sem acesso à internet para instalá-lo, além de Windows
Forms só rodar no Windows). Revisei a sintaxe manualmente, mas se algo
não compilar na sua máquina, me manda o erro que eu ajusto.
