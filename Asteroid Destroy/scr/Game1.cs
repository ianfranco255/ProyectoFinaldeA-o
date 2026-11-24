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

    // Sub-secciones internas dentro de la Tienda
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

        // --- TEXTURAS DEL MENÚ PRINCIPAL ---
        private Texture2D _fondoMenu;
        private Texture2D _btnIniciar;
        private Texture2D _btnTienda;
        private Texture2D _btnOpciones;
        private Texture2D _btnSalir;

        // =========================================================================
        // CAPAS DE LA TIENDA (ESTILO HOLLOW KNIGHT: DE ATRÁS HACIA ADELANTE)
        // =========================================================================
        private Texture2D _tiendaCapa1_Fondo;          // Fondo espacial profundo / nebulosa
        private Texture2D _tiendaCapa2_AgujeroNegro;   // Agujero negro central (soporta tira animada)
        private Texture2D _tiendaCapa3_MarcoHUD;       // Marco azul futurista superior e inferior

        // Capa 4: Sprites interactivos de botones laterales (NAVES, GACHA, ALIADOS, LLAVES, VOLVER)
        private Texture2D _btnTiendaNaves;
        private Texture2D _btnTiendaGacha;
        private Texture2D _btnTiendaAliados;
        private Texture2D _btnTiendaLlaves;
        private Texture2D _btnTiendaVolver;

        // Animación del Agujero Negro en la Tienda (Cambia _totalFotogramasAgujero si es tira animada)
        private int _totalFotogramasAgujero = 1; // Pon aquí cuántos cuadros tiene tu sprite del agujero negro (ej. 1 si es estático, 6 o 8 si es animado)
        private int _fotogramaAgujeroActual = 0;
        private float _cronometroAgujero = 0f;
        private float _tiempoFrameAgujero = 0.1f;

        // Control interactivo de la Tienda
        private Rectangle[] _rectangulosBotonesTienda;
        private int _opcionTiendaSeleccionada = 0; // 0: Naves, 1: Gacha, 2: Aliados, 3: Llaves, 4: Volver
        private CategoriaTienda _categoriaTiendaActiva = CategoriaTienda.Ninguna;
        private float _desplazamientoHoverTienda = 0f; // Efecto de deslizamiento lateral al seleccionar

        // --- TEXTURAS DEL NIVEL 1 ---
        private Texture2D _spriteFondo;
        private Texture2D _spriteNave;
        private Texture2D _spriteBala;
        private Texture2D _spriteAsteroide;

        // --- TEXTURAS DE INTERFAZ (HUD) ---
        private Texture2D _texturaPixel;
        private Texture2D _spriteCorazon;

        // --- VARIABLES DEL MENÚ PRINCIPAL ---
        private Rectangle[] _rectangulosBotones;
        private Color[] _coloresBaseBotones;
        private int _opcionSeleccionada = 0;
        private float _escalaPulso = 1.0f;
        private float _direccionPulso = 1.0f;
        private Point _ultimaPosicionMouse;

        // --- ENTIDADES DEL NIVEL 1 ---
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

        // Animación de la Nave
        private int _totalFotogramas = 8;
        private int _fotogramaActual = 0;
        private float _tiempoPorFotograma = 0.08f;
        private float _cronometroTiempo = 0f;
        private int _anchoFotograma;
        private int _altoFotograma;

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
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            _graphics.IsFullScreen = true;
            _graphics.ApplyChanges();

            ConfigurarBotonesMenu();
            ConfigurarBotonesTienda();
            ReiniciarPartida();

            _tecladoAnterior = Keyboard.GetState();
            _mouseAnterior = Mouse.GetState();
            _ultimaPosicionMouse = new Point(_mouseAnterior.X, _mouseAnterior.Y);

            base.Initialize();
        }

        private void ConfigurarBotonesMenu()
        {
            int anchoPantalla = _graphics.PreferredBackBufferWidth;
            int altoPantalla = _graphics.PreferredBackBufferHeight;

            int anchoBoton = (int)(anchoPantalla * 0.28f);
            int altoBoton = (int)(altoPantalla * 0.075f);
            int separacion = (int)(altoPantalla * 0.025f);

            int totalAltoMenu = (altoBoton * 4) + (separacion * 3);
            int inicioX = (anchoPantalla - anchoBoton) / 2;
            int inicioY = (altoPantalla - totalAltoMenu) / 2;

            _rectangulosBotones = new Rectangle[4];
            for (int i = 0; i < 4; i++)
            {
                _rectangulosBotones[i] = new Rectangle(
                    inicioX,
                    inicioY + i * (altoBoton + separacion),
                    anchoBoton,
                    altoBoton
                );
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

            // Proporciones basadas en tu boceto: 5 pestañas en fila vertical pegadas al borde izquierdo
            int anchoBoton = (int)(anchoPantalla * 0.24f);
            int altoBoton = (int)(altoPantalla * 0.085f);
            int separacion = (int)(altoPantalla * 0.028f);

            int totalAlto = (altoBoton * 5) + (separacion * 4);
            int inicioY = (altoPantalla - totalAlto) / 2;

            _rectangulosBotonesTienda = new Rectangle[5];
            for (int i = 0; i < 5; i++)
            {
                _rectangulosBotonesTienda[i] = new Rectangle(
                    0, // Pegado al margen izquierdo como en la imagen
                    inicioY + i * (altoBoton + separacion),
                    anchoBoton,
                    altoBoton
                );
            }
        }

        private void ReiniciarPartida()
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
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Texturas activas del Nivel 1
            _spriteFondo = Content.Load<Texture2D>("fondo");
            _spriteNave = Content.Load<Texture2D>("nave");
            _spriteBala = Content.Load<Texture2D>("bala");
            _spriteAsteroide = Content.Load<Texture2D>("asteroide");

            _texturaPixel = new Texture2D(GraphicsDevice, 1, 1);
            _texturaPixel.SetData(new[] { Color.White });
            _spriteCorazon = Content.Load<Texture2D>("corazon");
            _tiendaCapa2_AgujeroNegro = Content.Load<Texture2D>("tienda_agujero");
            _tiendaCapa1_Fondo = Content.Load<Texture2D>("tienda_fondo");
            _tiendaCapa3_MarcoHUD = Content.Load<Texture2D>("tienda_marco");




            // =========================================================================
            // CAPAS DE LA TIENDA: Descomenta las líneas cuyos sprites ya tengas en MGCB
            // =========================================================================
            

            // Botones interactivos de la Tienda:
            // _btnTiendaNaves = Content.Load<Texture2D>("btn_tienda_naves");
            // _btnTiendaGacha = Content.Load<Texture2D>("btn_tienda_gacha");
            // _btnTiendaAliados = Content.Load<Texture2D>("btn_tienda_aliados");
            // _btnTiendaLlaves = Content.Load<Texture2D>("btn_tienda_llaves");
            // _btnTiendaVolver = Content.Load<Texture2D>("btn_tienda_volver");

            // Otros sprites futuros:
            // _fondoMenu = Content.Load<Texture2D>("fondo_menu");
            // _btnIniciar = Content.Load<Texture2D>("btn_iniciar");
            // _btnTienda = Content.Load<Texture2D>("btn_tienda");
            // _btnOpciones = Content.Load<Texture2D>("btn_opciones");
            // _btnSalir = Content.Load<Texture2D>("btn_salir");
            // 

            _totalFotogramas = 8;
            _anchoFotograma = _spriteNave.Width / _totalFotogramas;
            _altoFotograma = _spriteNave.Height;
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
            _escalaPulso += 0.35f * _direccionPulso * deltaTiempo;
            if (_escalaPulso >= 1.08f) { _escalaPulso = 1.08f; _direccionPulso = -1f; }
            else if (_escalaPulso <= 0.96f) { _escalaPulso = 0.96f; _direccionPulso = 1f; }

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
            }
        }

        private void EjecutarAccionMenu(int opcion)
        {
            switch (opcion)
            {
                case 0:
                    ReiniciarPartida();
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

        // =========================================================================
        // LÓGICA INTERACTIVA DE LA TIENDA (TECLADO + MOUSE + ANIMACIÓN DE CAPAS)
        // =========================================================================
        private void ActualizarTienda(float deltaTiempo, KeyboardState teclado, MouseState mouse)
        {
            // Atajo rápido para volver al menú principal
            if ((teclado.IsKeyDown(Keys.Escape) && _tecladoAnterior.IsKeyUp(Keys.Escape)) ||
                (teclado.IsKeyDown(Keys.Back) && _tecladoAnterior.IsKeyUp(Keys.Back)))
            {
                _estadoActual = EstadoJuego.MenuPrincipal;
                return;
            }

            // Animación de fotogramas del Agujero Negro (Capa 2) si tiene más de 1 cuadro
            if (_totalFotogramasAgujero > 1)
            {
                _cronometroAgujero += deltaTiempo;
                if (_cronometroAgujero >= _tiempoFrameAgujero)
                {
                    _fotogramaAgujeroActual = (_fotogramaAgujeroActual + 1) % _totalFotogramasAgujero;
                    _cronometroAgujero = 0f;
                }
            }

            // Efecto sutil de deslizamiento hacia la derecha para el botón seleccionado
            _desplazamientoHoverTienda = (float)Math.Sin(DateTime.Now.TimeOfDay.TotalSeconds * 6.0) * 4f + 18f;

            // Navegación con Flechas (Arriba / Abajo) entre las 5 opciones
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

            // Interacción con el puntero del Mouse
            Point posMouseActual = new Point(mouse.X, mouse.Y);
            bool mouseSeMovio = posMouseActual != _ultimaPosicionMouse;
            _ultimaPosicionMouse = posMouseActual;

            for (int i = 0; i < _rectangulosBotonesTienda.Length; i++)
            {
                // Ampliamos ligeramente el área de detección hacia la derecha para compensar el desplazamiento hover
                Rectangle areaDeteccion = new Rectangle(
                    _rectangulosBotonesTienda[i].X,
                    _rectangulosBotonesTienda[i].Y,
                    _rectangulosBotonesTienda[i].Width + 25,
                    _rectangulosBotonesTienda[i].Height
                );

                if (areaDeteccion.Contains(posMouseActual))
                {
                    if (mouseSeMovio) _opcionTiendaSeleccionada = i;

                    if (mouse.LeftButton == ButtonState.Pressed && _mouseAnterior.LeftButton == ButtonState.Released)
                    {
                        _opcionTiendaSeleccionada = i;
                        EjecutarAccionTienda(i);
                        return;
                    }
                }
            }
        }

        private void EjecutarAccionTienda(int opcion)
        {
            switch (opcion)
            {
                case 0: // NAVES
                    _categoriaTiendaActiva = CategoriaTienda.Naves;
                    break;
                case 1: // GACHA
                    _categoriaTiendaActiva = CategoriaTienda.Gacha;
                    break;
                case 2: // ALIADOS
                    _categoriaTiendaActiva = CategoriaTienda.Aliados;
                    break;
                case 3: // LLAVES
                    _categoriaTiendaActiva = CategoriaTienda.Llaves;
                    break;
                case 4: // VOLVER
                    _estadoActual = EstadoJuego.MenuPrincipal;
                    break;
            }
        }

        private void ActualizarNivel1(float deltaTiempo, KeyboardState estadoTecladoActual)
        {
            if (estadoTecladoActual.IsKeyDown(Keys.Escape) && _tecladoAnterior.IsKeyUp(Keys.Escape))
            {
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

            bool estaMoviendose = false;

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
                    RadioColision = 4f
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

                    if (ast.Posicion.Y > ALTO_VIRTUAL + 10) ast.Activo = false;

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

                                if (ast.Vida <= 0) { ast.Activo = false; break; }
                            }
                        }
                    }

                    if (ast.Activo)
                    {
                        float distNaveX = _jugador.Posicion.X - ast.Posicion.X;
                        float distNaveY = _jugador.Posicion.Y - ast.Posicion.Y;
                        float distanciaNave = (float)Math.Sqrt(distNaveX * distNaveX + distNaveY * distNaveY);

                        if (distanciaNave < (_jugador.RadioColision + ast.RadioColision))
                        {
                            ast.Activo = false;

                            if (_jugador.TiempoInvulnerable <= 0f)
                            {
                                _jugador.Vidas--;
                                _jugador.TiempoInvulnerable = 1.5f;

                                if (_jugador.Vidas <= 0)
                                {
                                    ReiniciarPartida();
                                    return;
                                }
                            }
                        }
                    }
                }
                _listaAsteroides[i] = ast;
            }

            _listaBalas.RemoveAll(b => !b.Activa);
            _listaAsteroides.RemoveAll(a => !a.Activo);

            if (estaMoviendose)
            {
                _cronometroTiempo += deltaTiempo;
                if (_cronometroTiempo >= _tiempoPorFotograma)
                {
                    _fotogramaActual++;
                    if (_fotogramaActual >= _totalFotogramas) _fotogramaActual = 1;
                    _cronometroTiempo = 0f;
                }
            }
            else
            {
                _fotogramaActual = 0;
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            int anchoPantalla = _graphics.PreferredBackBufferWidth;
            int altoPantalla = _graphics.PreferredBackBufferHeight;

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp); // PointClamp mantiene tus pixel-arts nítidos sin borrosidad

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
            if (_fondoMenu != null)
                _spriteBatch.Draw(_fondoMenu, rectPantalla, Color.White);
            else if (_spriteFondo != null)
                _spriteBatch.Draw(_spriteFondo, rectPantalla, Color.White * 0.75f);

            Texture2D[] spritesBotones = { _btnIniciar, _btnTienda, _btnOpciones, _btnSalir };

            for (int i = 0; i < _rectangulosBotones.Length; i++)
            {
                bool estaSeleccionado = (i == _opcionSeleccionada);
                float escala = estaSeleccionado ? _escalaPulso : 1.0f;

                Rectangle rectBase = _rectangulosBotones[i];
                int anchoAnimado = (int)(rectBase.Width * escala);
                int altoAnimado = (int)(rectBase.Height * escala);
                int posX = rectBase.X + (rectBase.Width - anchoAnimado) / 2;
                int posY = rectBase.Y + (rectBase.Height - altoAnimado) / 2;

                Rectangle rectDestino = new Rectangle(posX, posY, anchoAnimado, altoAnimado);

                if (spritesBotones[i] != null)
                {
                    Color tinte = estaSeleccionado ? Color.White : Color.LightGray * 0.85f;
                    _spriteBatch.Draw(spritesBotones[i], rectDestino, tinte);
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

        // =========================================================================
        // RENDERIZADO DE LA TIENDA POR CAPAS (ESTILO HOLLOW KNIGHT)
        // =========================================================================
        private void DibujarTiendaPorCapas(int anchoPantalla, int altoPantalla)
        {
            Rectangle pantallaCompleta = new Rectangle(0, 0, anchoPantalla, altoPantalla);

            // ---------------------------------------------------------------------
            // CAPA 1 (FONDO LEJANO): Espacio oscuro o textura de fondo de tienda
            // ---------------------------------------------------------------------
            if (_tiendaCapa1_Fondo != null)
            {
                _spriteBatch.Draw(_tiendaCapa1_Fondo, pantallaCompleta, Color.White);
            }
            else
            {
                _spriteBatch.Draw(_texturaPixel, pantallaCompleta, Color.Black);
            }

            // ---------------------------------------------------------------------
            // CAPA 2 (ELEMENTO CENTRAL): Agujero Negro (Estático o Animado)
            // ---------------------------------------------------------------------
            if (_tiendaCapa2_AgujeroNegro != null)
            {
                int anchoCuadro = _tiendaCapa2_AgujeroNegro.Width / _totalFotogramasAgujero;
                int altoCuadro = _tiendaCapa2_AgujeroNegro.Height;
                Rectangle recorteAgujero = new Rectangle(_fotogramaAgujeroActual * anchoCuadro, 0, anchoCuadro, altoCuadro);

                // Posicionado ligeramente a la derecha del centro, igual que en tu imagen de referencia
                int anchoDestino = (int)(anchoPantalla * 0.68f);
                int altoDestino = (int)(altoPantalla * 0.85f);
                Rectangle destinoAgujero = new Rectangle(
                    (int)(anchoPantalla * 0.28f),
                    (altoPantalla - altoDestino) / 2,
                    anchoDestino,
                    altoDestino
                );

                _spriteBatch.Draw(_tiendaCapa2_AgujeroNegro, destinoAgujero, recorteAgujero, Color.White);
            }
            else
            {
                // Placeholder visual temporal del agujero negro diagonal
                Vector2 centroAgujero = new Vector2(anchoPantalla * 0.62f, altoPantalla * 0.5f);
                _spriteBatch.Draw(_texturaPixel, centroAgujero, null, Color.DarkOrange * 0.6f, -0.55f, new Vector2(0.5f, 0.5f), new Vector2(anchoPantalla * 0.65f, 45f), SpriteEffects.None, 0f);
                _spriteBatch.Draw(_texturaPixel, centroAgujero, null, Color.MediumOrchid * 0.8f, 0f, new Vector2(0.5f, 0.5f), new Vector2(220f, 220f), SpriteEffects.None, 0f);
                _spriteBatch.Draw(_texturaPixel, centroAgujero, null, Color.Black, 0f, new Vector2(0.5f, 0.5f), new Vector2(180f, 180f), SpriteEffects.None, 0f);
            }

            // ---------------------------------------------------------------------
            // CAPA 3 (ESTRUCTURA / MARCO HUD): Bordes azules superior e inferior
            // ---------------------------------------------------------------------
            if (_tiendaCapa3_MarcoHUD != null)
            {
                _spriteBatch.Draw(_tiendaCapa3_MarcoHUD, pantallaCompleta, Color.White);
            }
            else
            {
                // Placeholder temporal replicando el marco azul futurista superior e inferior de tu boceto
                Color azulMarco = new Color(28, 46, 130);
                Color azulBrillo = new Color(55, 85, 210);
                int grosorBarra = (int)(altoPantalla * 0.06f);

                // Barra superior
                _spriteBatch.Draw(_texturaPixel, new Rectangle(0, 0, anchoPantalla, grosorBarra), azulMarco);
                _spriteBatch.Draw(_texturaPixel, new Rectangle(0, 0, (int)(anchoPantalla * 0.35f), (int)(grosorBarra * 1.45f)), azulMarco);
                _spriteBatch.Draw(_texturaPixel, new Rectangle(0, grosorBarra, anchoPantalla, 4), azulBrillo);

                // Barra inferior
                _spriteBatch.Draw(_texturaPixel, new Rectangle(0, altoPantalla - grosorBarra, anchoPantalla, grosorBarra), azulMarco);
                _spriteBatch.Draw(_texturaPixel, new Rectangle((int)(anchoPantalla * 0.65f), altoPantalla - (int)(grosorBarra * 1.45f), (int)(anchoPantalla * 0.35f), (int)(grosorBarra * 1.45f)), azulMarco);
                _spriteBatch.Draw(_texturaPixel, new Rectangle(0, altoPantalla - grosorBarra - 4, anchoPantalla, 4), azulBrillo);
            }

            // ---------------------------------------------------------------------
            // CAPA 4 (INTERFAZ INTERACTIVA): Botones laterales (Naves, Gacha, Aliados, Llaves, Volver)
            // ---------------------------------------------------------------------
            Texture2D[] spritesTienda = { _btnTiendaNaves, _btnTiendaGacha, _btnTiendaAliados, _btnTiendaLlaves, _btnTiendaVolver };

            for (int i = 0; i < _rectangulosBotonesTienda.Length; i++)
            {
                bool seleccionado = (i == _opcionTiendaSeleccionada);
                Rectangle rectBase = _rectangulosBotonesTienda[i];

                // Al seleccionar o pasar el mouse, la pestaña se desliza suavemente hacia la derecha
                int offsetX = seleccionado ? (int)_desplazamientoHoverTienda : 0;
                Rectangle rectDestino = new Rectangle(rectBase.X + offsetX, rectBase.Y, rectBase.Width, rectBase.Height);

                if (spritesTienda[i] != null)
                {
                    Color tinte = seleccionado ? Color.White : Color.LightGray * 0.8f;
                    _spriteBatch.Draw(spritesTienda[i], rectDestino, tinte);
                }
                else
                {
                    // Placeholders replicando tus pestañas azules con el último botón (VOLVER) en tono rojizo
                    Color colorPestana = (i == 4) ? new Color(110, 25, 35) : new Color(32, 54, 155);
                    if (seleccionado)
                    {
                        colorPestana = (i == 4) ? new Color(210, 40, 40) : new Color(65, 115, 255);
                        _spriteBatch.Draw(_texturaPixel, new Rectangle(rectDestino.X, rectDestino.Y - 3, rectDestino.Width + 6, rectDestino.Height + 6), Color.White);
                    }

                    _spriteBatch.Draw(_texturaPixel, rectDestino, colorPestana);
                }
            }

            // ---------------------------------------------------------------------
            // CAPA 5 (PRIMER PLANO / MODAL DE CATEGORÍA ACTIVA)
            // Si el jugador hace clic en Naves, Gacha, Aliados o Llaves, aparece su panel aquí
            // ---------------------------------------------------------------------
            if (_categoriaTiendaActiva != CategoriaTienda.Ninguna)
            {
                int panelX = (int)(anchoPantalla * 0.30f);
                int panelY = (int)(altoPantalla * 0.14f);
                int panelW = (int)(anchoPantalla * 0.64f);
                int panelH = (int)(altoPantalla * 0.72f);

                Rectangle panelCategoria = new Rectangle(panelX, panelY, panelW, panelH);

                // Fondo semitransparente oscuro para dejar ver el agujero negro detrás (estilo Hollow Knight)
                _spriteBatch.Draw(_texturaPixel, new Rectangle(panelX - 3, panelY - 3, panelW + 6, panelH + 6), Color.CornflowerBlue * 0.8f);
                _spriteBatch.Draw(_texturaPixel, panelCategoria, new Color(10, 14, 35) * 0.88f);
            }
        }

        private void DibujarPantallaSecundaria(int anchoPantalla, int altoPantalla, Color colorPanel)
        {
            if (_fondoMenu != null)
                _spriteBatch.Draw(_fondoMenu, new Rectangle(0, 0, anchoPantalla, altoPantalla), Color.White * 0.35f);
            else if (_spriteFondo != null)
                _spriteBatch.Draw(_spriteFondo, new Rectangle(0, 0, anchoPantalla, altoPantalla), Color.White * 0.35f);

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
            Rectangle cuadroRecorte = new Rectangle(_fotogramaActual * _anchoFotograma, 0, _anchoFotograma, _altoFotograma);
            Vector2 centroNave = new Vector2(_anchoFotograma / 2f, _altoFotograma / 2f);

            int posYInt = (int)_posicionFondoY;
            Rectangle rectFondo1 = new Rectangle(0, posYInt, anchoPantalla, altoPantalla);
            Rectangle rectFondo2 = new Rectangle(0, posYInt - altoPantalla, anchoPantalla, altoPantalla);

            _spriteBatch.Draw(_spriteFondo, rectFondo1, Color.White);
            _spriteBatch.Draw(_spriteFondo, rectFondo2, Color.White);

            Vector2 centroAsteroide = new Vector2(_spriteAsteroide.Width / 2f, _spriteAsteroide.Height / 2f);
            foreach (var ast in _listaAsteroides)
            {
                if (ast.Activo)
                {
                    Vector2 posAstPixeles = new Vector2(ast.Posicion.X * factorX, ast.Posicion.Y * factorY);

                    Color colorDaño = Color.White;
                    if (ast.Vida == 2) colorDaño = Color.LightCoral;
                    if (ast.Vida == 1) colorDaño = Color.Red;

                    _spriteBatch.Draw(_spriteAsteroide, posAstPixeles, null, colorDaño, 0f, centroAsteroide, 1f, SpriteEffects.None, 0f);
                }
            }

            Vector2 centroBala = new Vector2(_spriteBala.Width / 2f, _spriteBala.Height / 2f);
            foreach (var b in _listaBalas)
            {
                if (b.Activa)
                {
                    Vector2 posicionBalaPixeles = new Vector2(b.Posicion.X * factorX, b.Posicion.Y * factorY);
                    _spriteBatch.Draw(_spriteBala, posicionBalaPixeles, null, Color.White, 0f, centroBala, 1f, SpriteEffects.None, 0f);
                }
            }

            Color colorNave = Color.White;
            if (_jugador.TiempoInvulnerable > 0f && (int)(_jugador.TiempoInvulnerable * 10) % 2 == 0)
            {
                colorNave = Color.Red * 0.6f;
            }
            _spriteBatch.Draw(_spriteNave, posicionNavePixeles, cuadroRecorte, colorNave, _jugador.Angulo, centroNave, 1f, SpriteEffects.None, 0f);

            DibujarInterfazHUD(anchoPantalla);
        }

        private void DibujarInterfazHUD(int anchoPantalla)
        {
            int margenDerecho = 25;
            int margenSuperior = 25;
            int tamanoCorazon = 36;
            int separacionCorazones = 10;

            int anchoTotalVidas = (VIDAS_MAXIMAS * tamanoCorazon) + ((VIDAS_MAXIMAS - 1) * separacionCorazones);
            int inicioX = anchoPantalla - margenDerecho - anchoTotalVidas;

            for (int i = 0; i < VIDAS_MAXIMAS; i++)
            {
                int posX = inicioX + i * (tamanoCorazon + separacionCorazones);
                Rectangle rectVida = new Rectangle(posX, margenSuperior, tamanoCorazon, tamanoCorazon);
                bool vidaActiva = i < _jugador.Vidas;

                if (_spriteCorazon != null)
                {
                    Color tinteCorazon = vidaActiva ? Color.White : Color.Black * 0.75f;
                    _spriteBatch.Draw(_spriteCorazon, rectVida, tinteCorazon);
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
