using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JuegoAsteroides
{
    internal struct Vector2Virtual
    {
        // Cambiamos a 'float' para que la rotación y el movimiento diagonal/angular sean suaves
        public float X;
        public float Y;
        public Vector2Virtual(float x, float y) { X = x; Y = y; }
    }

    internal struct Nave
    {
        public Vector2Virtual Posicion;
        public float Angulo; // NUEVO: Ángulo de rotación en radianes
    }

    internal struct Bala
    {
        public Vector2Virtual Posicion;
        public Vector2 Dirección; // NUEVO: Hacia dónde viaja la bala al ser disparada
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
        private float _velocidadNave = 1.5f; // Ajustada a float para el movimiento angular continuo
        private float _velocidadRotacion = 0.1f; // Velocidad de giro de la nave

        private KeyboardState _estadoTecladoAnterior;

        // Variables de Animación
        private int _fotogramasNave = 8;
        private int _fotogramasBala = 4;
        private int _fotogramaActualNave = 0;
        private int _fotogramaActualBala = 0;
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
            _jugador.Angulo = 0f; // Mirando hacia arriba por defecto (0 radianes)
            _proyectil.Activa = false;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteNave = Content.Load<Texture2D>("nave");
            _spriteBala = Content.Load<Texture2D>("bala");

            _anchoFotograma = _spriteNave.Width / _fotogramasNave;
            _altoFotograma = _spriteNave.Height;
            _anchoFotograma = _spriteBala.Width / _fotogramasBala;
            _altoFotograma = _spriteBala.Height;
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState estadoTecladoActual = Keyboard.GetState();

            if (estadoTecladoActual.IsKeyDown(Keys.Escape)) Exit();

            // ==========================================
            // NUEVO: ROTACIÓN CON FLECHAS (IZQ / DER)
            // ==========================================
            if (estadoTecladoActual.IsKeyDown(Keys.A))
            {
                _jugador.Angulo -= _velocidadRotacion;
            }
            if (estadoTecladoActual.IsKeyDown(Keys.D))
            {
                _jugador.Angulo += _velocidadRotacion;
            }

            // ==========================================
            // NUEVO: MOVIMIENTO DIRECCIONAL (W / S)
            // ==========================================
            // Calculamos los vectores de dirección según el ángulo actual de la nave
            // Restamos 90 grados (MathHelper.PiOver2) porque en 2D el ángulo 0 apunta a la derecha, 
            // y queremos que tu sprite mire hacia arriba por defecto.
            float direccionX = (float)Math.Cos(_jugador.Angulo - MathHelper.PiOver2);
            float direccionY = (float)Math.Sin(_jugador.Angulo - MathHelper.PiOver2);

            if (estadoTecladoActual.IsKeyDown(Keys.W))
            {
                // Avanzar hacia el frente
                _jugador.Posicion.X += direccionX * _velocidadNave;
                _jugador.Posicion.Y += direccionY * _velocidadNave;
            }
            if (estadoTecladoActual.IsKeyDown(Keys.S))
            {
                // Retroceder
                _jugador.Posicion.X -= direccionX * _velocidadNave;
                _jugador.Posicion.Y -= direccionY * _velocidadNave;
            }

            // Límites del mapa lógico
            if (_jugador.Posicion.X < 0) _jugador.Posicion.X = 0;
            if (_jugador.Posicion.X > ANCHO_VIRTUAL) _jugador.Posicion.X = ANCHO_VIRTUAL;
            if (_jugador.Posicion.Y < 0) _jugador.Posicion.Y = 0;
            if (_jugador.Posicion.Y > ALTO_VIRTUAL) _jugador.Posicion.Y = ALTO_VIRTUAL;

            // DISPARAR CON F (La bala hereda la dirección actual de la nave)
            if (estadoTecladoActual.IsKeyDown(Keys.F) && _estadoTecladoAnterior.IsKeyUp(Keys.F))
            {
                _proyectil.Activa = true;
                _proyectil.Posicion = new Vector2Virtual(_jugador.Posicion.X, _jugador.Posicion.Y);
                _proyectil.Dirección = new Vector2(direccionX, direccionY); // Guarda la dirección del disparo
            }

            // ACTUALIZAR PROYECTIL (Se mueve en su propia dirección guardada)
            if (_proyectil.Activa)
            {
                _proyectil.Posicion.X += _proyectil.Dirección.X * 3.0f; // Multiplicador de velocidad de bala
                _proyectil.Posicion.Y += _proyectil.Dirección.Y * 3.0f;

                if (_proyectil.Posicion.Y < 0 || _proyectil.Posicion.Y > ALTO_VIRTUAL ||
                    _proyectil.Posicion.X < 0 || _proyectil.Posicion.X > ANCHO_VIRTUAL)
                {
                    _proyectil.Activa = false;
                }
            }

            // Animación por tiempo
            _cronometroTiempo += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_cronometroTiempo >= _tiempoPorFotograma)
            {
                _fotogramaActualNave++;
                if (_fotogramaActualNave >= _fotogramasNave) _fotogramaActualNave = 0;
                _cronometroTiempo = 0f;
            }
            _cronometroTiempo += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_cronometroTiempo >= _tiempoPorFotograma)
            {
                _fotogramaActualBala++;
                if (_fotogramaActualBala >= _fotogramasBala) _fotogramaActualBala = 0;
                _cronometroTiempo = 0f;
            }

            _estadoTecladoAnterior = estadoTecladoActual;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();

            float factorX = _graphics.PreferredBackBufferWidth / (float)ANCHO_VIRTUAL;
            float factorY = _graphics.PreferredBackBufferHeight / (float)ALTO_VIRTUAL;

            // Dibujar Bala
            if (_proyectil.Activa)
            {
                Vector2 posicionBalaPixeles = new Vector2(_proyectil.Posicion.X * factorX, _proyectil.Posicion.Y * factorY);
                Vector2 centroBala = new Vector2(_spriteBala.Width / 2f, _spriteBala.Height / 2f);
                _spriteBatch.Draw(_spriteBala, posicionBalaPixeles, null, Color.White, 0f, centroBala, 1f, SpriteEffects.None, 0f);
            }

            // Dibujar Nave Animada y Rotada
            Vector2 posicionNavePixeles = new Vector2(_jugador.Posicion.X * factorX, _jugador.Posicion.Y * factorY);
            Vector2 centroNave = new Vector2(_anchoFotograma / 2f, _altoFotograma / 2f);
            Vector2 _posicionBalaPixeles = new Vector2(_jugador.Posicion.X * factorX, _jugador.Posicion.Y * factorY);
            Vector2 _centroBala = new Vector2(_anchoFotograma / 2f, _altoFotograma / 2f);


            // Límites de la ventana
            if (posicionNavePixeles.X < centroNave.X) posicionNavePixeles.X = centroNave.X;
            if (posicionNavePixeles.X > _graphics.PreferredBackBufferWidth - centroNave.X) posicionNavePixeles.X = _graphics.PreferredBackBufferWidth - centroNave.X;
            if (posicionNavePixeles.Y < centroNave.Y) posicionNavePixeles.Y = centroNave.Y;
            if (posicionNavePixeles.Y > _graphics.PreferredBackBufferHeight - centroNave.Y) posicionNavePixeles.Y = _graphics.PreferredBackBufferHeight - centroNave.Y;

            Rectangle cuadroRecorteNave = new Rectangle(_fotogramaActualNave * _anchoFotograma, 0, _anchoFotograma, _altoFotograma);
            Rectangle cuadroRecorteBala = new Rectangle(_fotogramaActualBala * _anchoFotograma, 0, _anchoFotograma, _altoFotograma);

            // ==========================================
            // MODIFICADO: Añadido '_jugador.Angulo' al Draw
            // ==========================================
            _spriteBatch.Draw(_spriteNave, posicionNavePixeles, cuadroRecorteNave, Color.White, _jugador.Angulo, centroNave, 1f, SpriteEffects.None, 0f);

            _spriteBatch.End();

            _spriteBatch.Draw(_spriteBala, _posicionBalaPixeles, cuadroRecorteBala, Color.White, _jugador.Angulo, _centroBala, 1f, SpriteEffects.None, 0f);

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
