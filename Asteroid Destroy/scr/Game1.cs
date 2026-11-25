using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace JuegoAsteroides
{
    public enum EstadoJuego
    {
        MenuPrincipal,
        Nivel1,
        Tienda,
        Opciones
    }

    public enum CategoriaTienda
    {
        Ninguna,
        Naves,
        Gacha,
        Aliados,
        Llaves
    }

    internal struct Vector2Virtual
    {
        public float X;
        public float Y;
        public Vector2Virtual(float x, float y) { X = x; Y = y; }
    }

    internal class SpriteAnimado
    {
        public Texture2D Textura;
        public int TotalFotogramas;
        public int FotogramaActual;
        public float TiempoPorFotograma;
        public float Cronometro;
        public bool BucleInfinito;
        public int FotogramaInicioBucle;

        public SpriteAnimado(Texture2D textura, int totalFotogramas, float tiempoPorFotograma = 0.08f, bool bucleInfinito = true, int fotogramaInicioBucle = 0)
        {
            Textura = textura;
            TotalFotogramas = Math.Max(1, totalFotogramas);
            FotogramaActual = 0;
            TiempoPorFotograma = tiempoPorFotograma;
            Cronometro = 0f;
            BucleInfinito = bucleInfinito;
            FotogramaInicioBucle = fotogramaInicioBucle;
        }

        public void Actualizar(float deltaTiempo)
        {
            if (Textura == null || TotalFotogramas <= 1) return;

            Cronometro += deltaTiempo;
            if (Cronometro >= TiempoPorFotograma)
            {
                Cronometro = 0f;
                FotogramaActual++;

                if (FotogramaActual >= TotalFotogramas)
                {
                    if (BucleInfinito)
                        FotogramaActual = FotogramaInicioBucle;
                    else
                        FotogramaActual = TotalFotogramas - 1;
                }
            }
        }

        public void Reiniciar()
        {
            FotogramaActual = 0;
            Cronometro = 0f;
        }

        public Rectangle ObtenerCuadroRecorte()
        {
            if (Textura == null) return Rectangle.Empty;
            int anchoFrame = Textura.Width / TotalFotogramas;
            int altoFrame = Textura.Height;
            return new Rectangle(FotogramaActual * anchoFrame, 0, anchoFrame, altoFrame);
        }

        public Vector2 ObtenerCentroFrame()
        {
            if (Textura == null) return Vector2.Zero;
            return new Vector2((Textura.Width / TotalFotogramas) / 2f, Textura.Height / 2f);
        }
    }

    internal struct Nave
    {
        public Vector2Virtual Posicion;
        public float Angulo;
        public float RadioColision;
        public int Vidas;
        public float TiempoInvulnerable;
    }

    internal struct Bala
    {
        public Vector2Virtual Posicion;
        public Vector2 Direccion;
        public bool Activa;
    }

    internal struct Asteroide
    {
        public Vector2Virtual Posicion;
        public Vector2 Direccion;
        public float Velocidad;
        public int Vida;
        public bool Activo;
        public float RadioColision;
        public float DistanciaMinimaNave;
        public bool FueDestruido;
    }

    internal struct PowerUpActivo
    {
        public string Nombre;
        public float TiempoRestante;
        public Color ColorRepresentativo;
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private EstadoJuego _estadoActual = EstadoJuego.MenuPrincipal;

        // =========================================================================
        // CONEXIÓN Y TELEMETRÍA CON BASE DE DATOS MYSQL
        // =========================================================================
        private DatabaseManager _db;
        private int _idJugadorActual = 1;
        private int _idSesionActual = -1;
        private float _tiempoSupervivencia = 0f;

        // =========================================================================
        // PANEL CENTRAL DE ESCALAS
        // =========================================================================
        private float _escalaBotonesMenu = 1.1f;
        private float _escalaBotonesTienda = 0.7f;
        private float _escalaAgujeroTienda = 3.5f;
        private float _escalaNave = 1.0f;
        private float _escalaBala = 1.0f;
        private float _escalaAsteroide = 2.5f;
        private float _escalaCorazonesHUD = 1.0f;

        // =========================================================================
        // SPRITES ANIMADOS (SPRITESHEETS) DE TODO EL JUEGO
        // =========================================================================
        private SpriteAnimado _animFondoMenu;
        private SpriteAnimado[] _animBotonesMenu;

        private SpriteAnimado _animTiendaCapa1_Fondo;
        private SpriteAnimado _animTiendaCapa2_AgujeroNegro;
        private SpriteAnimado _animTiendaCapa3_MarcoHUD;
        private SpriteAnimado[] _animBotonesTienda;

        private Texture2D _spriteFondoNivel1;
        private SpriteAnimado _animNave;
        private SpriteAnimado _animBala;
        private SpriteAnimado _animAsteroide;
        private SpriteAnimado _animCorazon;

        private Texture2D _texturaPixel;

        private Rectangle[] _rectangulosBotones;
        private Color[] _coloresBaseBotones;
        private int _opcionSeleccionada = 0;
        private Point _ultimaPosicionMouse;

        private Rectangle[] _rectangulosBotonesTienda;
        private int _opcionTiendaSeleccionada = 0;
        private CategoriaTienda _categoriaTiendaActiva = CategoriaTienda.Ninguna;

        private Nave _jugador;
        private List<Bala> _listaBalas;
        private List<Asteroide> _listaAsteroides;
        private List<PowerUpActivo> _powerUpsActivos;

        private float _posicionFondoY = 0f;
        private float _velocidadFondo = 80f;

        private float _cronometroDisparo = 0f;
        private float _cadenciaDisparo = 0.15f;

        private float _cronometroSpawn = 0f;
        private float _tiempoEntreAsteroides = 2.0f;
        private Random _random;

        private const int ANCHO_VIRTUAL = 100;
        private const int ALTO_VIRTUAL = 100;
        private const int VIDAS_MAXIMAS = 5;
        private float _velocidadNave = 1.5f;
        private float _velocidadRotacion = 0.05f;

        private KeyboardState _tecladoAnterior;
        private MouseState _mouseAnterior;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _listaBalas = new List<Bala>();
            _listaAsteroides = new List<Asteroide>();
            _powerUpsActivos = new List<PowerUpActivo>();
            _random = new Random();

            _animBotonesMenu = new SpriteAnimado[4];
            _animBotonesTienda = new SpriteAnimado[5];
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();

            // Inicializar conexión con MySQL y obtener/crear jugador
            _db = new DatabaseManager();
            _idJugadorActual = _db.ObtenerOCrearJugador("AstroPilot");

            ConfigurarBotonesMenu();
            ConfigurarBotonesTienda();
            ReiniciarPartida(registrarEnDb: false);

            _tecladoAnterior = Keyboard.GetState();
            _mouseAnterior = Mouse.GetState();
            _ultimaPosicionMouse = new Point(_mouseAnterior.X, _mouseAnterior.Y);

            base.Initialize();
        }

        private void ConfigurarBotonesMenu()
        {
            int anchoPantalla = _graphics.PreferredBackBufferWidth;
            int altoPantalla = _graphics.PreferredBackBufferHeight;

            // Tamaño proporcional a la pantalla (igual que los botones default)
            // Si quieres hacerlos más grandes o pequeños globalmente, ajusta estos porcentajes (ej. 0.32f y 0.085f)
            int anchoBoton = (int)(anchoPantalla * 0.3f);
            int altoBoton = (int)(altoPantalla * 0.08f);
            int separacion = (int)(altoPantalla * 0.025f);

            int totalAltoMenu = (altoBoton * 4) + (separacion * 3);
            int inicioX = (anchoPantalla - anchoBoton) / 2;
            int inicioY = (altoPantalla - totalAltoMenu) / 2;

            _rectangulosBotones = new Rectangle[4];
            for (int i = 0; i < 4; i++)
            {
                _rectangulosBotones[i] = new Rectangle(inicioX, inicioY + i * (altoBoton + separacion), anchoBoton, altoBoton);
            }

            _coloresBaseBotones = new Color[]
            {
                new Color(88, 52, 168),   // INICIAR
                new Color(255, 195, 11),  // TIENDA
                new Color(30, 167, 245),  // OPCIONES
                new Color(210, 30, 30)    // SALIR
            };
        }

        private void ConfigurarBotonesTienda()
        {
            int anchoPantalla = _graphics.PreferredBackBufferWidth;
            int altoPantalla = _graphics.PreferredBackBufferHeight;

            // Tamaño proporcional para las 5 pestañas laterales de la tienda
            int anchoBoton = (int)(anchoPantalla * 0.24f);
            int altoBoton = (int)(altoPantalla * 0.1f);
            int separacion = (int)(altoPantalla * 0.028f);

            int totalAlto = (altoBoton * 5) + (separacion * 4);
            int inicioY = (altoPantalla - totalAlto) / 2;

            _rectangulosBotonesTienda = new Rectangle[5];
            for (int i = 0; i < 5; i++)
            {
                _rectangulosBotonesTienda[i] = new Rectangle(0, inicioY + i * (altoBoton + separacion), anchoBoton, altoBoton);
            }
        }


        private void ReiniciarPartida(bool registrarEnDb = true)
        {
            _jugador.Posicion = new Vector2Virtual(50, 85);
            _jugador.Angulo = 0f;
            _jugador.RadioColision = 3.5f;
            _jugador.Vidas = VIDAS_MAXIMAS;
            _jugador.TiempoInvulnerable = 0f;

            _listaBalas.Clear();
            _listaAsteroides.Clear();
            _powerUpsActivos.Clear();
            _cronometroSpawn = 0f;
            _cronometroDisparo = _cadenciaDisparo;
            _tiempoSupervivencia = 0f;

            if (registrarEnDb && _db != null)
            {
                _idSesionActual = _db.IniciarSesion(_idJugadorActual, 1.0f);
            }
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _texturaPixel = new Texture2D(GraphicsDevice, 1, 1);
            _texturaPixel.SetData(new[] { Color.White });

            _spriteFondoNivel1 = Content.Load<Texture2D>("fondo");

            _animNave = new SpriteAnimado(Content.Load<Texture2D>("nave"), totalFotogramas: 9, tiempoPorFotograma: 0.08f, bucleInfinito: true, fotogramaInicioBucle: 1);
            _animBala = new SpriteAnimado(Content.Load<Texture2D>("bala"), totalFotogramas: 1, tiempoPorFotograma: 0.08f);
            _animAsteroide = new SpriteAnimado(Content.Load<Texture2D>("asteroide"), totalFotogramas: 4, tiempoPorFotograma: 0.10f);

            _animTiendaCapa1_Fondo = new SpriteAnimado(Content.Load<Texture2D>("tienda_fondo"), totalFotogramas: 5, tiempoPorFotograma: 0.12f);
            _animTiendaCapa2_AgujeroNegro = new SpriteAnimado(Content.Load<Texture2D>("tienda_agujero"), totalFotogramas: 5, tiempoPorFotograma: 0.09f);
            _animTiendaCapa3_MarcoHUD = new SpriteAnimado(Content.Load<Texture2D>("tienda_marco"), totalFotogramas: 5, tiempoPorFotograma: 0.15f);

            _animBotonesTienda[0] = new SpriteAnimado(Content.Load<Texture2D>("btn_tienda_naves"), totalFotogramas: 10, tiempoPorFotograma: 0.07f, bucleInfinito: true, fotogramaInicioBucle: 1);
            _animBotonesTienda[1] = new SpriteAnimado(Content.Load<Texture2D>("btn_tienda_gacha"), totalFotogramas: 1, tiempoPorFotograma: 0.07f, bucleInfinito: true, fotogramaInicioBucle: 1);
            _animBotonesTienda[2] = new SpriteAnimado(Content.Load<Texture2D>("btn_tienda_aliados"), totalFotogramas: 1, tiempoPorFotograma: 0.07f, bucleInfinito: true, fotogramaInicioBucle: 1);
            _animBotonesTienda[3] = new SpriteAnimado(Content.Load<Texture2D>("btn_tienda_llaves"), totalFotogramas: 10, tiempoPorFotograma: 0.07f, bucleInfinito: true, fotogramaInicioBucle: 1);
            _animBotonesTienda[4] = new SpriteAnimado(Content.Load<Texture2D>("btn_tienda_volver"), totalFotogramas: 10, tiempoPorFotograma: 0.07f, bucleInfinito: true, fotogramaInicioBucle: 1);

            _animBotonesMenu[0] = new SpriteAnimado(Content.Load<Texture2D>("btn_iniciar"), totalFotogramas: 1, tiempoPorFotograma: 0.07f, bucleInfinito: true, fotogramaInicioBucle: 1);
            _animBotonesMenu[1] = new SpriteAnimado(Content.Load<Texture2D>("btn_tienda"), totalFotogramas: 1, tiempoPorFotograma: 0.07f, bucleInfinito: true, fotogramaInicioBucle: 1);
            //_animBotonesMenu[2] = new SpriteAnimado(Content.Load<Texture2D>("btn_opciones"), totalFotogramas: 1, tiempoPorFotograma: 0.07f, bucleInfinito: true, fotogramaInicioBucle: 1);

            _animCorazon = new SpriteAnimado(Content.Load<Texture2D>("corazon"), totalFotogramas: 1, tiempoPorFotograma: 0.12f);

            ConfigurarBotonesMenu();
            ConfigurarBotonesTienda();
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTiempo = (float)gameTime.ElapsedGameTime.TotalSeconds;
            KeyboardState tecladoActual = Keyboard.GetState();
            MouseState mouseActual = Mouse.GetState();

            switch (_estadoActual)
            {
                case EstadoJuego.MenuPrincipal:
                    ActualizarMenuPrincipal(deltaTiempo, tecladoActual, mouseActual);
                    break;

                case EstadoJuego.Nivel1:
                    ActualizarNivel1(deltaTiempo, tecladoActual);
                    break;

                case EstadoJuego.Tienda:
                    ActualizarTienda(deltaTiempo, tecladoActual, mouseActual);
                    break;

                case EstadoJuego.Opciones:
                    if ((tecladoActual.IsKeyDown(Keys.Escape) && _tecladoAnterior.IsKeyUp(Keys.Escape)) ||
                        (tecladoActual.IsKeyDown(Keys.Back) && _tecladoAnterior.IsKeyUp(Keys.Back)))
                    {
                        _estadoActual = EstadoJuego.MenuPrincipal;
                    }
                    break;
            }

            _tecladoAnterior = tecladoActual;
            _mouseAnterior = mouseActual;
            base.Update(gameTime);
        }

        private void ActualizarMenuPrincipal(float deltaTiempo, KeyboardState teclado, MouseState mouse)
        {
            _animFondoMenu?.Actualizar(deltaTiempo);

            if (teclado.IsKeyDown(Keys.Down) && _tecladoAnterior.IsKeyUp(Keys.Down))
                _opcionSeleccionada = (_opcionSeleccionada + 1) % 4;
            if (teclado.IsKeyDown(Keys.Up) && _tecladoAnterior.IsKeyUp(Keys.Up))
                _opcionSeleccionada = (_opcionSeleccionada - 1 + 4) % 4;

            if ((teclado.IsKeyDown(Keys.Enter) && _tecladoAnterior.IsKeyUp(Keys.Enter)) ||
                (teclado.IsKeyDown(Keys.Space) && _tecladoAnterior.IsKeyUp(Keys.Space)))
            {
                EjecutarAccionMenu(_opcionSeleccionada);
                return;
            }

            Point posMouseActual = new Point(mouse.X, mouse.Y);
            bool mouseSeMovio = posMouseActual != _ultimaPosicionMouse;
            _ultimaPosicionMouse = posMouseActual;

            for (int i = 0; i < _rectangulosBotones.Length; i++)
            {
                if (_rectangulosBotones[i].Contains(posMouseActual))
                {
                    if (mouseSeMovio) _opcionSeleccionada = i;

                    if (mouse.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released)
                    {
                        _opcionSeleccionada = i;
                        EjecutarAccionMenu(i);
                        return;
                    }
                }

                if (_animBotonesMenu[i] != null)
                {
                    if (i == _opcionSeleccionada)
                        _animBotonesMenu[i].Actualizar(deltaTiempo);
                    else
                        _animBotonesMenu[i].Reiniciar();
                }
            }
        }

        private void EjecutarAccionMenu(int opcion)
        {
            switch (opcion)
            {
                case 0:
                    ReiniciarPartida(registrarEnDb: true);
                    _estadoActual = EstadoJuego.Nivel1;
                    break;
                case 1:
                    _opcionTiendaSeleccionada = 0;
                    _categoriaTiendaActiva = CategoriaTienda.Ninguna;
                    _estadoActual = EstadoJuego.Tienda;
                    break;
                case 2:
                    _estadoActual = EstadoJuego.Opciones;
                    break;
                case 3:
                    Exit();
                    break;
            }
        }

        private void ActualizarTienda(float deltaTiempo, KeyboardState teclado, MouseState mouse)
        {
            if ((teclado.IsKeyDown(Keys.Escape) && _tecladoAnterior.IsKeyUp(Keys.Escape)) ||
                (teclado.IsKeyDown(Keys.Back) && _tecladoAnterior.IsKeyUp(Keys.Back)))
            {
                _estadoActual = EstadoJuego.MenuPrincipal;
                return;
            }

            _animTiendaCapa1_Fondo?.Actualizar(deltaTiempo);
            _animTiendaCapa2_AgujeroNegro?.Actualizar(deltaTiempo);
            _animTiendaCapa3_MarcoHUD?.Actualizar(deltaTiempo);

            if (teclado.IsKeyDown(Keys.Down) && _tecladoAnterior.IsKeyUp(Keys.Down))
                _opcionTiendaSeleccionada = (_opcionTiendaSeleccionada + 1) % 5;
            if (teclado.IsKeyDown(Keys.Up) && _tecladoAnterior.IsKeyUp(Keys.Up))
                _opcionTiendaSeleccionada = (_opcionTiendaSeleccionada - 1 + 5) % 5;

            if ((teclado.IsKeyDown(Keys.Enter) && _tecladoAnterior.IsKeyUp(Keys.Enter)) ||
                (teclado.IsKeyDown(Keys.Space) && _tecladoAnterior.IsKeyUp(Keys.Space)))
            {
                EjecutarAccionTienda(_opcionTiendaSeleccionada);
                return;
            }

            Point posMouseActual = new Point(mouse.X, mouse.Y);
            bool mouseSeMovio = posMouseActual != _ultimaPosicionMouse;
            _ultimaPosicionMouse = posMouseActual;

            for (int i = 0; i < _rectangulosBotonesTienda.Length; i++)
            {
                if (_rectangulosBotonesTienda[i].Contains(posMouseActual))
                {
                    if (mouseSeMovio) _opcionTiendaSeleccionada = i;

                    if (mouse.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released)
                    {
                        _opcionTiendaSeleccionada = i;
                        EjecutarAccionTienda(i);
                        return;
                    }
                }

                if (_animBotonesTienda[i] != null)
                {
                    if (i == _opcionTiendaSeleccionada)
                        _animBotonesTienda[i].Actualizar(deltaTiempo);
                    else
                        _animBotonesTienda[i].Reiniciar();
                }
            }
        }

        private void EjecutarAccionTienda(int opcion)
        {
            switch (opcion)
            {
                case 0: _categoriaTiendaActiva = CategoriaTienda.Naves; break;
                case 1: _categoriaTiendaActiva = CategoriaTienda.Gacha; break;
                case 2: _categoriaTiendaActiva = CategoriaTienda.Aliados; break;
                case 3: _categoriaTiendaActiva = CategoriaTienda.Llaves; break;
                case 4: _estadoActual = EstadoJuego.MenuPrincipal; break;
            }
        }

        private void ActualizarNivel1(float deltaTiempo, KeyboardState estadoTecladoActual)
        {
            _tiempoSupervivencia += deltaTiempo;

            if (estadoTecladoActual.IsKeyDown(Keys.Escape) && _tecladoAnterior.IsKeyUp(Keys.Escape))
            {
                _db?.FinalizarSesion(_idSesionActual, (int)_tiempoSupervivencia);
                _estadoActual = EstadoJuego.MenuPrincipal;
                return;
            }

            int altoPantalla = _graphics.PreferredBackBufferHeight;
            _posicionFondoY += _velocidadFondo * deltaTiempo;
            if (_posicionFondoY >= altoPantalla) _posicionFondoY -= altoPantalla;

            if (_jugador.TiempoInvulnerable > 0f) _jugador.TiempoInvulnerable -= deltaTiempo;

            for (int i = _powerUpsActivos.Count - 1; i >= 0; i--)
            {
                PowerUpActivo pu = _powerUpsActivos[i];
                pu.TiempoRestante -= deltaTiempo;
                if (pu.TiempoRestante <= 0f) _powerUpsActivos.RemoveAt(i);
                else _powerUpsActivos[i] = pu;
            }

            if (estadoTecladoActual.IsKeyDown(Keys.Left)) _jugador.Angulo -= _velocidadRotacion;
            if (estadoTecladoActual.IsKeyDown(Keys.Right)) _jugador.Angulo += _velocidadRotacion;

            float direccionX = (float)Math.Cos(_jugador.Angulo - MathHelper.PiOver2);
            float direccionY = (float)Math.Sin(_jugador.Angulo - MathHelper.PiOver2);

            float lateralX = (float)Math.Cos(_jugador.Angulo);
            float lateralY = (float)Math.Sin(_jugador.Angulo);

            bool estaMoviendose = true;

            if (estadoTecladoActual.IsKeyDown(Keys.W))
            {
                _jugador.Posicion.X += direccionX * _velocidadNave;
                _jugador.Posicion.Y += direccionY * _velocidadNave;
                estaMoviendose = true;
            }
            if (estadoTecladoActual.IsKeyDown(Keys.S))
            {
                _jugador.Posicion.X -= direccionX * _velocidadNave;
                _jugador.Posicion.Y -= direccionY * _velocidadNave;
                estaMoviendose = true;
            }
            if (estadoTecladoActual.IsKeyDown(Keys.A))
            {
                _jugador.Posicion.X -= lateralX * _velocidadNave;
                _jugador.Posicion.Y -= lateralY * _velocidadNave;
                estaMoviendose = true;
            }
            if (estadoTecladoActual.IsKeyDown(Keys.D))
            {
                _jugador.Posicion.X += lateralX * _velocidadNave;
                _jugador.Posicion.Y += lateralY * _velocidadNave;
                estaMoviendose = true;
            }

            if (_jugador.Posicion.X < 0) _jugador.Posicion.X = 0;
            if (_jugador.Posicion.X > ANCHO_VIRTUAL) _jugador.Posicion.X = ANCHO_VIRTUAL;
            if (_jugador.Posicion.Y < 0) _jugador.Posicion.Y = 0;
            if (_jugador.Posicion.Y > ALTO_VIRTUAL) _jugador.Posicion.Y = ALTO_VIRTUAL;

            _cronometroDisparo += deltaTiempo;
            if (estadoTecladoActual.IsKeyDown(Keys.LeftShift) && _cronometroDisparo >= _cadenciaDisparo)
            {
                Bala nuevaBala = new Bala
                {
                    Activa = true,
                    Posicion = new Vector2Virtual(_jugador.Posicion.X, _jugador.Posicion.Y),
                    Direccion = new Vector2(direccionX, direccionY)
                };
                _listaBalas.Add(nuevaBala);
                _cronometroDisparo = 0f;
            }

            for (int i = 0; i < _listaBalas.Count; i++)
            {
                Bala b = _listaBalas[i];
                if (b.Activa)
                {
                    b.Posicion.X += b.Direccion.X * 3.0f;
                    b.Posicion.Y += b.Direccion.Y * 3.0f;

                    if (b.Posicion.Y < 0 || b.Posicion.Y > ALTO_VIRTUAL ||
                        b.Posicion.X < 0 || b.Posicion.X > ANCHO_VIRTUAL)
                    {
                        b.Activa = false;
                    }
                }
                _listaBalas[i] = b;
            }

            _cronometroSpawn += deltaTiempo;
            if (_cronometroSpawn >= _tiempoEntreAsteroides)
            {
                _cronometroSpawn = 0f;
                Asteroide nuevoAsteroide = new Asteroide
                {
                    Posicion = new Vector2Virtual(_random.Next(5, ANCHO_VIRTUAL - 5), -5),
                    Direccion = new Vector2((float)(_random.NextDouble() * 2 - 1), 1f),
                    Velocidad = (float)(_random.NextDouble() * 0.5f + 0.3f),
                    Vida = 3,
                    Activo = true,
                    RadioColision = 4f,
                    DistanciaMinimaNave = 999f,
                    FueDestruido = false
                };
                nuevoAsteroide.Direccion.Normalize();
                _listaAsteroides.Add(nuevoAsteroide);
            }

            for (int i = 0; i < _listaAsteroides.Count; i++)
            {
                Asteroide ast = _listaAsteroides[i];
                if (ast.Activo)
                {
                    ast.Posicion.X += ast.Direccion.X * ast.Velocidad;
                    ast.Posicion.Y += ast.Direccion.Y * ast.Velocidad;

                    if (ast.Posicion.X <= 0) { ast.Posicion.X = 0; ast.Direccion.X *= -1; }
                    else if (ast.Posicion.X >= ANCHO_VIRTUAL) { ast.Posicion.X = ANCHO_VIRTUAL; ast.Direccion.X *= -1; }

                    // Calcular distancia actual a la nave para rastrear la cercanía mínima
                    float distNaveX = _jugador.Posicion.X - ast.Posicion.X;
                    float distNaveY = _jugador.Posicion.Y - ast.Posicion.Y;
                    float distanciaNave = (float)Math.Sqrt(distNaveX * distNaveX + distNaveY * distNaveY);

                    if (distanciaNave < ast.DistanciaMinimaNave)
                    {
                        ast.DistanciaMinimaNave = distanciaNave;
                    }

                    // Colisión Bala vs Asteroide
                    for (int j = 0; j < _listaBalas.Count; j++)
                    {
                        Bala b = _listaBalas[j];
                        if (b.Activa)
                        {
                            float distX = b.Posicion.X - ast.Posicion.X;
                            float distY = b.Posicion.Y - ast.Posicion.Y;
                            float distancia = (float)Math.Sqrt(distX * distX + distY * distY);

                            if (distancia < ast.RadioColision)
                            {
                                ast.Vida--;
                                b.Activa = false;
                                _listaBalas[j] = b;

                                if (ast.Vida <= 0)
                                {
                                    ast.Activo = false;
                                    ast.FueDestruido = true;
                                    break;
                                }
                            }
                        }
                    }

                    // Colisión Nave vs Asteroide
                    if (ast.Activo && distanciaNave < (_jugador.RadioColision + ast.RadioColision))
                    {
                        ast.Activo = false;
                        ast.FueDestruido = true;

                        if (_jugador.TiempoInvulnerable <= 0f)
                        {
                            _jugador.Vidas--;
                            _jugador.TiempoInvulnerable = 1.5f;

                            string coord = $"{(int)_jugador.Posicion.X},{(int)_jugador.Posicion.Y}";
                            _db?.RegistrarColision(_idSesionActual, 1, coord, 20);

                            if (_jugador.Vidas <= 0)
                            {
                                _db?.FinalizarSesion(_idSesionActual, (int)_tiempoSupervivencia);
                                ReiniciarPartida(registrarEnDb: true);
                                return;
                            }
                        }
                    }

                    // Registro automático de EVASIÓN si sale por abajo sin tocar la nave ni ser destruido
                    if (ast.Activo && ast.Posicion.Y > ALTO_VIRTUAL + 10)
                    {
                        ast.Activo = false;
                        if (!ast.FueDestruido)
                        {
                            _db?.RegistrarEvasion(_idSesionActual, 1, (decimal)Math.Round(ast.DistanciaMinimaNave, 2));
                        }
                    }
                }
                _listaAsteroides[i] = ast;
            }

            _listaBalas.RemoveAll(b => !b.Activa);
            _listaAsteroides.RemoveAll(a => !a.Activo);

            if (estaMoviendose)
                _animNave.Actualizar(deltaTiempo);
            else
                _animNave.Reiniciar();

            _animBala.Actualizar(deltaTiempo);
            _animAsteroide.Actualizar(deltaTiempo);
            _animCorazon?.Actualizar(deltaTiempo);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            int anchoPantalla = _graphics.PreferredBackBufferWidth;
            int altoPantalla = _graphics.PreferredBackBufferHeight;

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            switch (_estadoActual)
            {
                case EstadoJuego.MenuPrincipal:
                    DibujarMenuPrincipal(anchoPantalla, altoPantalla);
                    break;

                case EstadoJuego.Nivel1:
                    DibujarNivel1(anchoPantalla, altoPantalla);
                    break;

                case EstadoJuego.Tienda:
                    DibujarTiendaPorCapas(anchoPantalla, altoPantalla);
                    break;

                case EstadoJuego.Opciones:
                    DibujarPantallaSecundaria(anchoPantalla, altoPantalla, new Color(20, 50, 80));
                    break;
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void DibujarMenuPrincipal(int anchoPantalla, int altoPantalla)
        {
            Rectangle rectPantalla = new Rectangle(0, 0, anchoPantalla, altoPantalla);
            if (_animFondoMenu != null && _animFondoMenu.Textura != null)
                _spriteBatch.Draw(_animFondoMenu.Textura, rectPantalla, _animFondoMenu.ObtenerCuadroRecorte(), Color.White);
            else if (_spriteFondoNivel1 != null)
                _spriteBatch.Draw(_spriteFondoNivel1, rectPantalla, Color.White * 0.75f);

            for (int i = 0; i < _rectangulosBotones.Length; i++)
            {
                bool estaSeleccionado = (i == _opcionSeleccionada);
                Rectangle rectDestino = _rectangulosBotones[i];

                if (_animBotonesMenu[i] != null && _animBotonesMenu[i].Textura != null)
                {
                    _spriteBatch.Draw(_animBotonesMenu[i].Textura, rectDestino, _animBotonesMenu[i].ObtenerCuadroRecorte(), Color.White);
                }
                else
                {
                    if (estaSeleccionado)
                    {
                        Rectangle rectBorde = new Rectangle(rectDestino.X - 4, rectDestino.Y - 4, rectDestino.Width + 8, rectDestino.Height + 8);
                        _spriteBatch.Draw(_texturaPixel, rectBorde, Color.White);
                    }

                    Color colorBoton = estaSeleccionado ? _coloresBaseBotones[i] : _coloresBaseBotones[i] * 0.65f;
                    _spriteBatch.Draw(_texturaPixel, rectDestino, colorBoton);
                }
            }
        }

        private void DibujarTiendaPorCapas(int anchoPantalla, int altoPantalla)
        {
            Rectangle pantallaCompleta = new Rectangle(0, 0, anchoPantalla, altoPantalla);

            if (_animTiendaCapa1_Fondo != null && _animTiendaCapa1_Fondo.Textura != null)
            {
                _spriteBatch.Draw(_animTiendaCapa1_Fondo.Textura, pantallaCompleta, _animTiendaCapa1_Fondo.ObtenerCuadroRecorte(), Color.White);
            }
            else
            {
                _spriteBatch.Draw(_texturaPixel, pantallaCompleta, Color.Black);
            }

            if (_animTiendaCapa2_AgujeroNegro != null && _animTiendaCapa2_AgujeroNegro.Textura != null)
            {
                Rectangle recorteAgujero = _animTiendaCapa2_AgujeroNegro.ObtenerCuadroRecorte();
                int anchoAgujero = (int)(recorteAgujero.Width * _escalaAgujeroTienda);
                int altoAgujero = (int)(recorteAgujero.Height * _escalaAgujeroTienda);

                Rectangle destinoAgujero = new Rectangle(
                    (int)(anchoPantalla * 0.62f) - (anchoAgujero / 2),
                    (altoPantalla - altoAgujero) / 2,
                    anchoAgujero,
                    altoAgujero
                );

                _spriteBatch.Draw(_animTiendaCapa2_AgujeroNegro.Textura, destinoAgujero, recorteAgujero, Color.White);
            }
            else
            {
                Vector2 centroAgujero = new Vector2(anchoPantalla * 0.62f, altoPantalla * 0.5f);
                _spriteBatch.Draw(_texturaPixel, centroAgujero, null, Color.DarkOrange * 0.6f, -0.55f, new Vector2(0.5f, 0.5f), new Vector2(anchoPantalla * 0.65f, 45f), SpriteEffects.None, 0f);
                _spriteBatch.Draw(_texturaPixel, centroAgujero, null, Color.MediumOrchid * 0.8f, 0f, new Vector2(0.5f, 0.5f), new Vector2(220f, 220f), SpriteEffects.None, 0f);
                _spriteBatch.Draw(_texturaPixel, centroAgujero, null, Color.Black, 0f, new Vector2(0.5f, 0.5f), new Vector2(180f, 180f), SpriteEffects.None, 0f);
            }

            if (_animTiendaCapa3_MarcoHUD != null && _animTiendaCapa3_MarcoHUD.Textura != null)
            {
                _spriteBatch.Draw(_animTiendaCapa3_MarcoHUD.Textura, pantallaCompleta, _animTiendaCapa3_MarcoHUD.ObtenerCuadroRecorte(), Color.White);
            }
            else
            {
                Color azulMarco = new Color(28, 46, 130);
                Color azulBrillo = new Color(55, 85, 210);
                int grosorBarra = (int)(altoPantalla * 0.06f);

                _spriteBatch.Draw(_texturaPixel, new Rectangle(0, 0, anchoPantalla, grosorBarra), azulMarco);
                _spriteBatch.Draw(_texturaPixel, new Rectangle(0, 0, (int)(anchoPantalla * 0.35f), (int)(grosorBarra * 1.45f)), azulMarco);
                _spriteBatch.Draw(_texturaPixel, new Rectangle(0, grosorBarra, anchoPantalla, 4), azulBrillo);

                _spriteBatch.Draw(_texturaPixel, new Rectangle(0, altoPantalla - grosorBarra, anchoPantalla, grosorBarra), azulMarco);
                _spriteBatch.Draw(_texturaPixel, new Rectangle((int)(anchoPantalla * 0.65f), altoPantalla - (int)(grosorBarra * 1.45f), (int)(anchoPantalla * 0.35f), (int)(grosorBarra * 1.45f)), azulMarco);
                _spriteBatch.Draw(_texturaPixel, new Rectangle(0, altoPantalla - grosorBarra - 4, anchoPantalla, 4), azulBrillo);
            }

            for (int i = 0; i < _rectangulosBotonesTienda.Length; i++)
            {
                bool seleccionado = (i == _opcionTiendaSeleccionada);
                Rectangle rectDestino = _rectangulosBotonesTienda[i];

                if (_animBotonesTienda[i] != null && _animBotonesTienda[i].Textura != null)
                {
                    _spriteBatch.Draw(_animBotonesTienda[i].Textura, rectDestino, _animBotonesTienda[i].ObtenerCuadroRecorte(), Color.White);
                }
                else
                {
                    Color colorPestana = (i == 4) ? new Color(110, 25, 35) : new Color(32, 54, 155);
                    if (seleccionado)
                    {
                        colorPestana = (i == 4) ? new Color(210, 40, 40) : new Color(65, 115, 255);
                        _spriteBatch.Draw(_texturaPixel, new Rectangle(rectDestino.X, rectDestino.Y - 3, rectDestino.Width + 6, rectDestino.Height + 6), Color.White);
                    }
                    _spriteBatch.Draw(_texturaPixel, rectDestino, colorPestana);
                }
            }

            if (_categoriaTiendaActiva != CategoriaTienda.Ninguna)
            {
                int panelX = (int)(anchoPantalla * 0.30f);
                int panelY = (int)(altoPantalla * 0.14f);
                int panelW = (int)(anchoPantalla * 0.64f);
                int panelH = (int)(altoPantalla * 0.72f);

                Rectangle panelCategoria = new Rectangle(panelX, panelY, panelW, panelH);
                _spriteBatch.Draw(_texturaPixel, new Rectangle(panelX - 3, panelY - 3, panelW + 6, panelH + 6), Color.CornflowerBlue * 0.8f);
                _spriteBatch.Draw(_texturaPixel, panelCategoria, new Color(10, 14, 35) * 0.88f);
            }
        }

        private void DibujarPantallaSecundaria(int anchoPantalla, int altoPantalla, Color colorPanel)
        {
            if (_animFondoMenu != null && _animFondoMenu.Textura != null)
                _spriteBatch.Draw(_animFondoMenu.Textura, new Rectangle(0, 0, anchoPantalla, altoPantalla), _animFondoMenu.ObtenerCuadroRecorte(), Color.White * 0.35f);
            else if (_spriteFondoNivel1 != null)
                _spriteBatch.Draw(_spriteFondoNivel1, new Rectangle(0, 0, anchoPantalla, altoPantalla), Color.White * 0.35f);

            int margenX = (int)(anchoPantalla * 0.1f);
            int margenY = (int)(altoPantalla * 0.1f);
            Rectangle panel = new Rectangle(margenX, margenY, anchoPantalla - margenX * 2, altoPantalla - margenY * 2);

            _spriteBatch.Draw(_texturaPixel, new Rectangle(panel.X - 4, panel.Y - 4, panel.Width + 8, panel.Height + 8), Color.White * 0.7f);
            _spriteBatch.Draw(_texturaPixel, panel, colorPanel);
        }

        private void DibujarNivel1(int anchoPantalla, int altoPantalla)
        {
            float factorX = anchoPantalla / (float)ANCHO_VIRTUAL;
            float factorY = altoPantalla / (float)ALTO_VIRTUAL;

            Vector2 posicionNavePixeles = new Vector2(_jugador.Posicion.X * factorX, _jugador.Posicion.Y * factorY);

            int posYInt = (int)_posicionFondoY;
            Rectangle rectFondo1 = new Rectangle(0, posYInt, anchoPantalla, altoPantalla);
            Rectangle rectFondo2 = new Rectangle(0, posYInt - altoPantalla, anchoPantalla, altoPantalla);

            _spriteBatch.Draw(_spriteFondoNivel1, rectFondo1, Color.White);
            _spriteBatch.Draw(_spriteFondoNivel1, rectFondo2, Color.White);

            Vector2 centroAsteroide = _animAsteroide.ObtenerCentroFrame();
            Rectangle recorteAsteroide = _animAsteroide.ObtenerCuadroRecorte();
            foreach (var ast in _listaAsteroides)
            {
                if (ast.Activo)
                {
                    Vector2 posAstPixeles = new Vector2(ast.Posicion.X * factorX, ast.Posicion.Y * factorY);
                    Color colorDaño = Color.White;
                    if (ast.Vida == 2) colorDaño = Color.LightCoral;
                    if (ast.Vida == 1) colorDaño = Color.Red;

                    _spriteBatch.Draw(_animAsteroide.Textura, posAstPixeles, recorteAsteroide, colorDaño, 0f, centroAsteroide, _escalaAsteroide, SpriteEffects.None, 0f);
                }
            }

            Vector2 centroBala = _animBala.ObtenerCentroFrame();
            Rectangle recorteBala = _animBala.ObtenerCuadroRecorte();
            foreach (var b in _listaBalas)
            {
                if (b.Activa)
                {
                    Vector2 posicionBalaPixeles = new Vector2(b.Posicion.X * factorX, b.Posicion.Y * factorY);
                    _spriteBatch.Draw(_animBala.Textura, posicionBalaPixeles, recorteBala, Color.White, 0f, centroBala, _escalaBala, SpriteEffects.None, 0f);
                }
            }

            Color colorNave = Color.White;
            if (_jugador.TiempoInvulnerable > 0f && (int)(_jugador.TiempoInvulnerable * 10) % 2 == 0)
            {
                colorNave = Color.Red * 0.6f;
            }
            _spriteBatch.Draw(_animNave.Textura, posicionNavePixeles, _animNave.ObtenerCuadroRecorte(), colorNave, _jugador.Angulo, _animNave.ObtenerCentroFrame(), _escalaNave, SpriteEffects.None, 0f);

            DibujarInterfazHUD(anchoPantalla);
        }

        private void DibujarInterfazHUD(int anchoPantalla)
        {
            int margenDerecho = 25;
            int margenSuperior = 25;
            int tamanoCorazon = (int)(36 * _escalaCorazonesHUD);
            int separacionCorazones = 10;

            int anchoTotalVidas = (VIDAS_MAXIMAS * tamanoCorazon) + ((VIDAS_MAXIMAS - 1) * separacionCorazones);
            int inicioX = anchoPantalla - margenDerecho - anchoTotalVidas;

            for (int i = 0; i < VIDAS_MAXIMAS; i++)
            {
                int posX = inicioX + i * (tamanoCorazon + separacionCorazones);
                Rectangle rectVida = new Rectangle(posX, margenSuperior, tamanoCorazon, tamanoCorazon);
                bool vidaActiva = i < _jugador.Vidas;

                if (_animCorazon != null && _animCorazon.Textura != null)
                {
                    Color tinteCorazon = vidaActiva ? Color.White : Color.Black * 0.75f;
                    _spriteBatch.Draw(_animCorazon.Textura, rectVida, _animCorazon.ObtenerCuadroRecorte(), tinteCorazon);
                }
                else
                {
                    Rectangle rectBorde = new Rectangle(rectVida.X - 2, rectVida.Y - 2, rectVida.Width + 4, rectVida.Height + 4);
                    _spriteBatch.Draw(_texturaPixel, rectBorde, Color.White * 0.6f);

                    Color colorRelleno = vidaActiva ? Color.Crimson : Color.Black;
                    _spriteBatch.Draw(_texturaPixel, rectVida, colorRelleno);
                }
            }

            int tamanoSlotPowerUp = 42;
            int separacionPowerUps = 12;
            int posXPowerUp = anchoPantalla - margenDerecho - tamanoSlotPowerUp;
            int inicioYPowerUp = margenSuperior + tamanoCorazon + 20;

            int slotsVisibles = Math.Max(3, _powerUpsActivos.Count);
            for (int i = 0; i < slotsVisibles; i++)
            {
                int posY = inicioYPowerUp + i * (tamanoSlotPowerUp + separacionPowerUps);
                Rectangle rectSlot = new Rectangle(posXPowerUp, posY, tamanoSlotPowerUp, tamanoSlotPowerUp);

                Rectangle rectMarco = new Rectangle(rectSlot.X - 2, rectSlot.Y - 2, rectSlot.Width + 4, rectSlot.Height + 4);
                _spriteBatch.Draw(_texturaPixel, rectMarco, Color.Gray * 0.5f);

                if (i < _powerUpsActivos.Count)
                {
                    _spriteBatch.Draw(_texturaPixel, rectSlot, _powerUpsActivos[i].ColorRepresentativo);
                }
                else
                {
                    _spriteBatch.Draw(_texturaPixel, rectSlot, Color.Black * 0.45f);
                }
            }
        }
    }
}
