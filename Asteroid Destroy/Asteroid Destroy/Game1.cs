using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JuegoAsteroides
{
    internal struct Vector2Virtual
    {
        public float X;
        public float Y;
        public Vector2Virtual(float x, float y) { X = x; Y = y; }
    }

    internal struct Nave
    {
        public Vector2Virtual Posicion;
        public float Angulo;
    }

    internal struct Bala
    {
        public Vector2Virtual Posicion;
        public Vector2 Dirección;
        public bool Activa;
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _spriteNave;
        private Texture2D _spriteBala;

        private Nave _jugador;
        private Bala _proyectil;

        private const int ANCHO_VIRTUAL = 100;
        private const int ALTO_VIRTUAL = 100;
        private float _velocidadNave = 2f;
        private float _velocidadRotacion = 0.1f;

        private KeyboardState _estadoTecladoAnterior;

        // Variables de Animación
        private int _totalFotogramas = 8;
        private int _fotogramaActual = 0;
        private float _tiempoPorFotograma = 0.08f;
        private float _cronometroTiempo = 0f;
        private int _anchoFotograma;
        private int _altoFotograma;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 800;
        }

        protected override void Initialize()
        {
            _jugador.Posicion = new Vector2Virtual(50, 85);
            _jugador.Angulo = 0f;
            _proyectil.Activa = false;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteNave = Content.Load<Texture2D>("nave");
            _spriteBala = Content.Load<Texture2D>("bala");

            _anchoFotograma = _spriteNave.Width / _totalFotogramas;
            _altoFotograma = _spriteNave.Height;
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState estadoTecladoActual = Keyboard.GetState();

            if (estadoTecladoActual.IsKeyDown(Keys.Escape)) Exit();

            // Rotación con flechas
            if (estadoTecladoActual.IsKeyDown(Keys.A)) _jugador.Angulo -= _velocidadRotacion;
            if (estadoTecladoActual.IsKeyDown(Keys.D)) _jugador.Angulo += _velocidadRotacion;

            // Movimiento direccional
            float direccionX = (float)Math.Cos(_jugador.Angulo - MathHelper.PiOver2);
            float direccionY = (float)Math.Sin(_jugador.Angulo - MathHelper.PiOver2);

            if (estadoTecladoActual.IsKeyDown(Keys.W))
            {
                _jugador.Posicion.X += direccionX * _velocidadNave;
                _jugador.Posicion.Y += direccionY * _velocidadNave;
            }
            if (estadoTecladoActual.IsKeyDown(Keys.S))
            {
                _jugador.Posicion.X -= direccionX * _velocidadNave;
                _jugador.Posicion.Y -= direccionY * _velocidadNave;
            }

            // CORRECCIÓN: Restricción del universo virtual hecha en Update antes de dibujar
            if (_jugador.Posicion.X < 0) _jugador.Posicion.X = 0;
            if (_jugador.Posicion.X > ANCHO_VIRTUAL) _jugador.Posicion.X = ANCHO_VIRTUAL;
            if (_jugador.Posicion.Y < 0) _jugador.Posicion.Y = 0;
            if (_jugador.Posicion.Y > ALTO_VIRTUAL) _jugador.Posicion.Y = ALTO_VIRTUAL;

            // Disparar
            if (estadoTecladoActual.IsKeyDown(Keys.F) && _estadoTecladoAnterior.IsKeyUp(Keys.F))
            {
                _proyectil.Activa = true;
                _proyectil.Posicion = new Vector2Virtual(_jugador.Posicion.X, _jugador.Posicion.Y);
                _proyectil.Dirección = new Vector2(direccionX, direccionY);
            }

            // Actualizar proyectil
            if (_proyectil.Activa)
            {
                _proyectil.Posicion.X += _proyectil.Dirección.X * 3.0f;
                _proyectil.Posicion.Y += _proyectil.Dirección.Y * 3.0f;

                if (_proyectil.Posicion.Y < 0 || _proyectil.Posicion.Y > ALTO_VIRTUAL ||
                    _proyectil.Posicion.X < 0 || _proyectil.Posicion.X > ANCHO_VIRTUAL)
                {
                    _proyectil.Activa = false;
                }
            }

            // Animación
            _cronometroTiempo += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_cronometroTiempo >= _tiempoPorFotograma)
            {
                _fotogramaActual++;
                if (_fotogramaActual >= _totalFotogramas) _fotogramaActual = 0;
                _cronometroTiempo = 0f;
            }

            _estadoTecladoAnterior = estadoTecladoActual;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Obtenemos los factores de escala
            float factorX = _graphics.PreferredBackBufferWidth / (float)ANCHO_VIRTUAL;
            float factorY = _graphics.PreferredBackBufferHeight / (float)ALTO_VIRTUAL;

            // Calculamos posiciones de renderizado antes de abrir el SpriteBatch
            Vector2 posicionNavePixeles = new Vector2(_jugador.Posicion.X * factorX, _jugador.Posicion.Y * factorY);
            Vector2 centroNave = new Vector2(_anchoFotograma / 2f, _altoFotograma / 2f);
            Rectangle cuadroRecorte = new Rectangle(_fotogramaActual * _anchoFotograma, 0, _anchoFotograma, _altoFotograma);

            // CORRECCIÓN: Bloque de renderizado limpio e ininterrumpido
            _spriteBatch.Begin();

            // Dibujar Bala si está activa
            if (_proyectil.Activa)
            {
                Vector2 posicionBalaPixeles = new Vector2(_proyectil.Posicion.X * factorX, _proyectil.Posicion.Y * factorY);
                Vector2 centroBala = new Vector2(_spriteBala.Width / 2f, _spriteBala.Height / 2f);
                _spriteBatch.Draw(_spriteBala, posicionBalaPixeles, null, Color.White, 0f, centroBala, 1f, SpriteEffects.None, 0f);
            }

            // Dibujar Nave
            _spriteBatch.Draw(_spriteNave, posicionNavePixeles, cuadroRecorte, Color.White, _jugador.Angulo, centroNave, 1f, SpriteEffects.None, 0f);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
