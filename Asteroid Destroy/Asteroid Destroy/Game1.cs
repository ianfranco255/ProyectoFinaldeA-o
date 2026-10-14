using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JuegoAsteroides
{
    // Arquitectura de Estados de Juego para separar el menú, la tienda y la partida
    public enum EstadoPantalla
    {
        MenuPrincipal,
        PartidaIniciada,
        Tienda
    }

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

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // ESTADO DE PANTALLA ACTUAL
        private EstadoPantalla _estadoActual = EstadoPantalla.MenuPrincipal;

        // SPRITES Y TEXTURAS
        private Texture2D _spriteNave;
        // NOTA: Declaramos los nombres de los nuevos sprites para el menú interactivo.
        // Los cargarás en LoadContent cuando los tengas en Pixilart.
        private Texture2D _spriteBotonJugar;
        private Texture2D _spriteBotonTienda;
        private Texture2D _spriteBotonSalir;

        // ELEMENTOS DEL MENÚ INTERACTIVO
        private int _opcionSeleccionada = 0; // 0 = Jugar, 1 = Tienda, 2 = Salir
        private Rectangle[] _areasBotones;  // Rectángulos físicos de colisión para el mouse
        private float _animacionEscalaBoton = 1f;
        private float _direccionAnimacion = 1f;

        // VARIABLES DE LA NAVE (PARTIDA)
        private Nave _jugador;
        private const int ANCHO_VIRTUAL = 100;
        private const int ALTO_VIRTUAL = 100;
        private float _velocidadNave = 1.5f;
        private float _velocidadRotacion = 0.05f;

        // CONTROLES
        private KeyboardState _estadoTecladoAnterior;
        private MouseState _estadoMouseAnterior;

        // ANIMACIÓN DE LA NAVE
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
            IsMouseVisible = true; // Hacemos el cursor visible para el menú interactivo
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 800;
        }

        protected override void Initialize()
        {
            _jugador.Posicion = new Vector2Virtual(50, 50); // Iniciamos en el centro
            _jugador.Angulo = 0f;

            // Definimos las posiciones en pantalla de los 3 botones del menú (Centrados horizontalmente)
            _areasBotones = new Rectangle[3];
            _areasBotones[0] = new Rectangle(250, 250, 300, 80); // Botón Jugar
            _areasBotones[1] = new Rectangle(250, 370, 300, 80); // Botón Tienda
            _areasBotones[2] = new Rectangle(250, 490, 300, 80); // Botón Salir

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _spriteNave = Content.Load<Texture2D>("nave");

            _anchoFotograma = _spriteNave.Width / _totalFotogramas;
            _altoFotograma = _spriteNave.Height;

            // =================================================================================
            // DESARROLLADOR TRABAJANDO: Marcadores de posición para tus texturas de menú.
            // Cuando agregues tus PNGs al MGCB como hiciste con la nave, descomenta estas líneas:
            // =================================================================================
            // _spriteBotonJugar = Content.Load<Texture2D>("boton_jugar");
            // _spriteBotonTienda = Content.Load<Texture2D>("boton_tienda");
            // _spriteBotonSalir = Content.Load<Texture2D>("boton_salir");
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState estadoTecladoActual = Keyboard.GetState();
            MouseState estadoMouseActual = Mouse.GetState();

            // Salida rápida global con ESC
            if (estadoTecladoActual.IsKeyDown(Keys.Escape)) Exit();

            // Animación sutil de latido/pulso para la opción seleccionada del menú
            _animacionEscalaBoton += 0.005f * _direccionAnimacion;
            if (_animacionEscalaBoton > 1.05f || _animacionEscalaBoton < 0.95f)
                _direccionAnimacion *= -1;

            // DERIVACIÓN DE LÓGICAS DEPENDIENDO DEL ESTADO
            switch (_estadoActual)
            {
                case EstadoPantalla.MenuPrincipal:
                    ActualizarMenu(estadoTecladoActual, estadoMouseActual);
                    break;

                case EstadoPantalla.PartidaIniciada:
                    ActualizarPartida(gameTime, estadoTecladoActual);
                    break;

                case EstadoPantalla.Tienda:
                    ActualizarTienda(estadoTecladoActual);
                    break;
            }

            _estadoTecladoAnterior = estadoTecladoActual;
            _estadoMouseAnterior = estadoMouseActual;
            base.Update(gameTime);
        }

        private void ActualizarMenu(KeyboardState teclado, MouseState mouse)
        {
            Point posicionMouse = new Point(mouse.X, mouse.Y);

            // 1. Control por Mouse (Interacción por Puntero)
            for (int i = 0; i < _areasBotones.Length; i++)
            {
                if (_areasBotones[i].Contains(posicionMouse))
                {
                    if (_opcionSeleccionada != i) _opcionSeleccionada = i; // Cambia selección al pasar el mouse

                    if (mouse.LeftButton == ButtonState.Pressed && _estadoMouseAnterior.LeftButton == ButtonState.Released)
                    {
                        EjecutarOpcionMenu(_opcionSeleccionada);
                        return;
                    }
                }
            }

            // 2. Control por Teclado (Flechas)
            if (teclado.IsKeyDown(Keys.Down) && _estadoTecladoAnterior.IsKeyUp(Keys.Down))
            {
                _opcionSeleccionada = (_opcionSeleccionada + 1) % 3;
            }
            if (teclado.IsKeyDown(Keys.Up) && _estadoTecladoAnterior.IsKeyUp(Keys.Up))
            {
                _opcionSeleccionada = (_opcionSeleccionada - 1 + 3) % 3;
            }

            // Confirmar selección con Enter o Espacio
            if ((teclado.IsKeyDown(Keys.Enter) && _estadoTecladoAnterior.IsKeyUp(Keys.Enter)) ||
                (teclado.IsKeyDown(Keys.Space) && _estadoTecladoAnterior.IsKeyUp(Keys.Space)))
            {
                EjecutarOpcionMenu(_opcionSeleccionada);
            }
        }

        private void EjecutarOpcionMenu(int opcion)
        {
            if (opcion == 0) _estadoActual = EstadoPantalla.PartidaIniciada;
            else if (opcion == 1) _estadoActual = EstadoPantalla.Tienda;
            else if (opcion == 2) Exit();
        }

        private void ActualizarPartida(GameTime gameTime, KeyboardState teclado)
        {
            // REQUERIMIENTO: Rotar libremente con las flechas del teclado (Izquierda / Derecha)
            if (teclado.IsKeyDown(Keys.Left)) _jugador.Angulo -= _velocidadRotacion;
            if (teclado.IsKeyDown(Keys.Right)) _jugador.Angulo += _velocidadRotacion;

            // REQUERIMIENTO: Desplazarse libremente usando WASD de forma fluida y lineal
            if (teclado.IsKeyDown(Keys.W)) _jugador.Posicion.Y -= _velocidadNave;
            if (teclado.IsKeyDown(Keys.S)) _jugador.Posicion.Y += _velocidadNave;
            if (teclado.IsKeyDown(Keys.A)) _jugador.Posicion.X -= _velocidadNave;
            if (teclado.IsKeyDown(Keys.D)) _jugador.Posicion.X += _velocidadNave;

            // Restricción estricta del mapa
            if (_jugador.Posicion.X < 0) _jugador.Posicion.X = 0;
            if (_jugador.Posicion.X > ANCHO_VIRTUAL) _jugador.Posicion.X = ANCHO_VIRTUAL;
            if (_jugador.Posicion.Y < 0) _jugador.Posicion.Y = 0;
            if (_jugador.Posicion.Y > ALTO_VIRTUAL) _jugador.Posicion.Y = ALTO_VIRTUAL;

            // Actualización de fotogramas de la nave
            _cronometroTiempo += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_cronometroTiempo >= _tiempoPorFotograma)
            {
                _fotogramaActual = (_fotogramaActual + 1) % _totalFotogramas;
                _cronometroTiempo = 0f;
            }
        }

        private void ActualizarTienda(KeyboardState teclado)
        {
            // Volver al menú principal desde la tienda presionando la tecla de retroceso (Backspace)
            if (teclado.IsKeyDown(Keys.Back) && _estadoTecladoAnterior.IsKeyUp(Keys.Back))
            {
                _estadoActual = EstadoPantalla.MenuPrincipal;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black); // Espacio profundo

            _spriteBatch.Begin();

            switch (_estadoActual)
            {
                case EstadoPantalla.MenuPrincipal:
                    DibujarMenu();
                    break;

                case EstadoPantalla.PartidaIniciada:
                    DibujarPartida();
                    break;

                case EstadoPantalla.Tienda:
                    DibujarTienda();
                    break;
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }

            // NOTA PROFESIONAL: Mientras preparas los sprites artísticos del menú, el motor dibujará
        private void DibujarMenu()
        {
            // Rectángulos de colores planos para probar la interactividad de inmediato
            for (int i = 0; i < _areasBotones.Length; i++)
            {
                Color colorBoton = (i == _opcionSeleccionada) ? Color.Cyan : Color.DarkSlateGray;
                float escala = (i == _opcionSeleccionada) ? _animacionEscalaBoton : 1f;

                // Calculamos el origen de escalado para que el botón lata desde su propio centro
                Vector2 centroBoton = new Vector2(_areasBotones[i].Width / 2f, _areasBotones[i].Height / 2f);
                Vector2 posicionRender = new Vector2(_areasBotones[i].X + centroBoton.X, _areasBotones[i].Y + centroBoton.Y);

                Texture2D texturaAAplicar = null;
                if (i == 0) texturaAAplicar = _spriteBotonJugar;
                if (i == 1) texturaAAplicar = _spriteBotonTienda;
                if (i == 2) texturaAAplicar = _spriteBotonSalir;

                if (texturaAAplicar != null)
                {
                    // Cuando asignes tus sprites PNG reales, se dibujarán aquí con la animación de tamaño
                    _spriteBatch.Draw(texturaAAplicar, posicionRender, null, Color.White, 0f, centroBoton, escala, SpriteEffects.None, 0f);
                }
                else
                {
                    // Render temporal seguro: Crea un bloque de color interactivo si no hay imagen asignada
                    Texture2D rectanguloPlano = new Texture2D(GraphicsDevice, 1, 1);
                    rectanguloPlano.SetData(new[] { Color.White });

                    // Multiplicamos la escala por el tamaño para darle la forma de botón rectangular correcta
                    Vector2 escalaEstructural = new Vector2(_areasBotones[i].Width * escala, _areasBotones[i].Height * escala);
                    _spriteBatch.Draw(rectanguloPlano, posicionRender, null, colorBoton, 0f, centroBoton, escalaEstructural, SpriteEffects.None, 0f);
                }
            }
        }

        private void DibujarPartida()
        {
            float factorX = _graphics.PreferredBackBufferWidth / (float)ANCHO_VIRTUAL;
            float factorY = _graphics.PreferredBackBufferHeight / (float)ALTO_VIRTUAL;

            Vector2 posicionNavePixeles = new Vector2(_jugador.Posicion.X * factorX, _jugador.Posicion.Y * factorY);
            Vector2 centroNave = new Vector2(_anchoFotograma / 2f, _altoFotograma / 2f);
            Rectangle cuadroRecorte = new Rectangle(_fotogramaActual * _anchoFotograma, 0, _anchoFotograma, _altoFotograma);

            _spriteBatch.Draw(_spriteNave, posicionNavePixeles, cuadroRecorte, Color.White, _jugador.Angulo, centroNave, 1f, SpriteEffects.None, 0f);
        }

        private void DibujarTienda()
        {
            // Interfaz de desarrollo temporal para la tienda
            Texture2D fondoTienda = new Texture2D(GraphicsDevice, 1, 1);
            fondoTienda.SetData(new[] { Color.Purple });
            _spriteBatch.Draw(fondoTienda, new Rectangle(50, 50, 700, 700), Color.DarkMagenta * 0.5f);
        }
    }
}
