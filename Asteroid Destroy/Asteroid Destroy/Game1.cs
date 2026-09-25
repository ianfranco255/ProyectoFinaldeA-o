using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JuegoAsteroides
{
    // Mantenemos tus estructuras lógicas originales
    internal struct Vector2Virtual
    {
        public int X;
        public int Y;
        public Vector2Virtual(int x, int y) { X = x; Y = y; }
    }

    internal struct Nave
    {
        public Vector2Virtual Posicion;
    }

    internal struct Bala
    {
        public Vector2Virtual Posicion;
        public bool Activa;
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // TUS TEXTURAS DE PIXILART
        private Texture2D _spriteNave;
        private Texture2D _spriteBala;

        //Control de velocidad
        private int _velocidadNave = 2;

        // Estado del juego basado en tu lógica original
        private Nave _jugador;
        private Bala _proyectil;

        // Límites del mapa lógico de referencia que definiste
        private const int ANCHO_VIRTUAL = 100;
        private const int ALTO_VIRTUAL = 100;

        // Estados de teclado para evitar disparos repetidos infinitos (ráfaga)
        private KeyboardState _estadoTecladoAnterior;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            // Configuración de la ventana emergente (Resolución de pantalla)
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 800;
        }

        protected override void Initialize()
        {
            // Inicialización de coordenadas idénticas a tu código de consola
            _jugador.Posicion = new Vector2Virtual(50, 85);
            _proyectil.Activa = false;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // CARGA DE SPRITES: Busca "nave.png" y "bala.png" en la carpeta Content
            // Nota: Si usas la herramienta MGCB, no pongas la extensión .png en el texto
            _spriteNave = Content.Load<Texture2D>("nave");
            _spriteBala = Content.Load<Texture2D>("bala");
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState estadoTecladoActual = Keyboard.GetState();

            // Salir del juego (ESC)
            if (estadoTecladoActual.IsKeyDown(Keys.Escape))
                Exit();

            // PROCESAR ENTRADA (Tu lógica original WASD)
            // Ahora ambas direcciones usan el control centralizado de velocidad
            int pasoX = _velocidadNave;
            int pasoY = _velocidadNave;

            if (estadoTecladoActual.IsKeyDown(Keys.W)) _jugador.Posicion.Y -= pasoY;
            if (estadoTecladoActual.IsKeyDown(Keys.S)) _jugador.Posicion.Y += pasoY;
            if (estadoTecladoActual.IsKeyDown(Keys.A)) _jugador.Posicion.X -= pasoX;
            if (estadoTecladoActual.IsKeyDown(Keys.D)) _jugador.Posicion.X += pasoX;


            // Restricción matemática estricta sobre el universo virtual (0 a 100)
            if (_jugador.Posicion.X < 0) _jugador.Posicion.X = 0;
            if (_jugador.Posicion.X > ANCHO_VIRTUAL) _jugador.Posicion.X = ANCHO_VIRTUAL;
            if (_jugador.Posicion.Y < 0) _jugador.Posicion.Y = 0;
            if (_jugador.Posicion.Y > ALTO_VIRTUAL) _jugador.Posicion.Y = ALTO_VIRTUAL;

            // Disparar con F (Detecta solo la pulsación inicial para evitar ráfaga descontrolada)
            if (estadoTecladoActual.IsKeyDown(Keys.F) && _estadoTecladoAnterior.IsKeyUp(Keys.F))
            {
                _proyectil.Activa = true;
                _proyectil.Posicion = new Vector2Virtual(_jugador.Posicion.X, _jugador.Posicion.Y);
            }

            // ACTUALIZAR JUEGO (Movimiento del proyectil)
            if (_proyectil.Activa)
            {
                _proyectil.Posicion.Y -= 8; // La bala avanza

                if (_proyectil.Posicion.Y < 0)
                {
                    _proyectil.Activa = false;
                }
            }

            _estadoTecladoAnterior = estadoTecladoActual;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Color de fondo azul espacial
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();

            // 1. REGLA DE TRES: Mapear tu universo (0-100) al tamaño real de la ventana (800x800)
            float factorX = _graphics.PreferredBackBufferWidth / (float)ANCHO_VIRTUAL;
            float factorY = _graphics.PreferredBackBufferHeight / (float)ALTO_VIRTUAL;

            // 2. CORRECCIÓN PARA LA BALA (Si está activa)
            if (_proyectil.Activa)
            {
                // Calculamos la posición en píxeles basándonos en tu lógica
                Vector2 posicionBalaPixeles = new Vector2(_proyectil.Posicion.X * factorX, _proyectil.Posicion.Y * factorY);

                // Obtenemos la mitad del tamaño de la bala para centrar el sprite
                Vector2 centroBala = new Vector2(_spriteBala.Width / 2f, _spriteBala.Height / 2f);

                // Dibujamos usando el origen centrado
                _spriteBatch.Draw(_spriteBala, posicionBalaPixeles, null, Color.White, 0f, centroBala, 1f, SpriteEffects.None, 0f);
            }

            // 3. CORRECCIÓN PARA LA NAVE
            // Calculamos la posición en píxeles basándonos en tu lógica
            Vector2 posicionNavePixeles = new Vector2(_jugador.Posicion.X * factorX, _jugador.Posicion.Y * factorY);

            // Obtenemos la mitad del tamaño de la nave para centrar el sprite
            Vector2 centroNave = new Vector2(_spriteNave.Width / 2f, _spriteNave.Height / 2f);

            // Ajuste dinámico de los límites físicos en pantalla para que la nave NUNCA se salga del marco:
            // Evita que la parte izquierda, derecha, superior o inferior del sprite cruce los bordes de la ventana
            if (posicionNavePixeles.X < centroNave.X)
                posicionNavePixeles.X = centroNave.X;
            if (posicionNavePixeles.X > _graphics.PreferredBackBufferWidth - centroNave.X)
                posicionNavePixeles.X = _graphics.PreferredBackBufferWidth - centroNave.X;
            if (posicionNavePixeles.Y < centroNave.Y)
                posicionNavePixeles.Y = centroNave.Y;
            if (posicionNavePixeles.Y > _graphics.PreferredBackBufferHeight - centroNave.Y)
                posicionNavePixeles.Y = _graphics.PreferredBackBufferHeight - centroNave.Y;

            // Dibujamos la nave usando el origen centrado
            _spriteBatch.Draw(_spriteNave, posicionNavePixeles, null, Color.White, 0f, centroNave, 1f, SpriteEffects.None, 0f);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

    }
}
