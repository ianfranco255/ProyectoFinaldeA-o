using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace JuegoAsteroides
{
    internal struct Vector2
    {
        public int X;
        public int Y;

        public Vector2(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    internal struct Nave
    {
        public Vector2 Posicion;
        public int UltimoX;
        public int UltimoY;
    }

    internal struct Bala
    {
        public Vector2 Posicion;
        public Vector2 Direccion;
        public bool Activa;
        public int UltimoX;
        public int UltimoY;
    }

    internal struct PantallaConfig
    {
        public int AnchoConsola;
        public int AltoConsola;
        public double RelacionAspecto;
        public int LimiteMaxX;
        public int LimiteMaxY;
    }

    internal struct EstadoJuego
    {
        public bool Jugando;
        public PantallaConfig Pantalla;
        public Nave Jugador;
        public Bala Proyectil;
    }

    internal struct Program
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        private static extern bool DeleteMenu(IntPtr hMenu, uint uPosition, uint uFlags);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        private const uint SC_SIZE = 0xF000;

        private static EstadoJuego _estado;

        private const int ANCHO_VIRTUAL = 100;
        private const int ALTO_VIRTUAL = 100;

        static void Main(string[] args)
        {
            BloquearRedimensionamientoMouse();
            ConfigurarConsola();

            while (_estado.Jugando)
            {
                VerificarCambioTamaño();
                ProcesarEntrada();
                ActualizarJuego();
                Renderizar();

                Thread.Sleep(33); // ~30 FPS
            }
        }

        static void BloquearRedimensionamientoMouse()
        {
            IntPtr handleVentana = GetConsoleWindow();
            IntPtr handleMenu = GetSystemMenu(handleVentana, false);
            DeleteMenu(handleMenu, SC_SIZE, 0x00000000);
        }

        static void ConfigurarConsola()
        {
            Console.Title = "Asteroids - Renderizado de Balas Sincronizado";
            Console.CursorVisible = false;
            Console.Clear();

            _estado.Jugando = true;

            _estado.Jugador.Posicion = new Vector2(50, 85);
            _estado.Jugador.UltimoX = -1;
            _estado.Jugador.UltimoY = -1;

            _estado.Proyectil.Activa = false;
            _estado.Proyectil.UltimoX = -1;
            _estado.Proyectil.UltimoY = -1;

            ActualizarDimensionesPantalla();
        }

        static void VerificarCambioTamaño()
        {
            if (Console.WindowWidth != _estado.Pantalla.AnchoConsola ||
                Console.WindowHeight != _estado.Pantalla.AltoConsola)
            {
                Console.Clear();
                ActualizarDimensionesPantalla();
            }
        }

        static void ActualizarDimensionesPantalla()
        {
            _estado.Pantalla.AnchoConsola = Console.WindowWidth;
            _estado.Pantalla.AltoConsola = Console.WindowHeight;

            if (_estado.Pantalla.AltoConsola == 0) _estado.Pantalla.AltoConsola = 1;
            if (_estado.Pantalla.AnchoConsola == 0) _estado.Pantalla.AnchoConsola = 1;

            _estado.Pantalla.RelacionAspecto = (double)_estado.Pantalla.AnchoConsola / _estado.Pantalla.AltoConsola;

            _estado.Pantalla.LimiteMaxX = _estado.Pantalla.AnchoConsola - 2;
            _estado.Pantalla.LimiteMaxY = _estado.Pantalla.AltoConsola - 2;
        }

        static void ProcesarEntrada()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo tecla = Console.ReadKey(true);

                if (tecla.Key == ConsoleKey.Escape) _estado.Jugando = false;

                int pasoX = 4;
                int pasoY = 4;

                if (tecla.Key == ConsoleKey.W) _estado.Jugador.Posicion.Y -= pasoY;
                if (tecla.Key == ConsoleKey.S) _estado.Jugador.Posicion.Y += pasoY;
                if (tecla.Key == ConsoleKey.A) _estado.Jugador.Posicion.X -= pasoX;
                if (tecla.Key == ConsoleKey.D) _estado.Jugador.Posicion.X += pasoX;

                if (_estado.Jugador.Posicion.X < 0) _estado.Jugador.Posicion.X = 0;
                if (_estado.Jugador.Posicion.X > ANCHO_VIRTUAL) _estado.Jugador.Posicion.X = ANCHO_VIRTUAL;
                if (_estado.Jugador.Posicion.Y < 0) _estado.Jugador.Posicion.Y = 0;
                if (_estado.Jugador.Posicion.Y > ALTO_VIRTUAL) _estado.Jugador.Posicion.Y = ALTO_VIRTUAL;

                // Disparar con F
                if (tecla.Key == ConsoleKey.F && !_estado.Proyectil.Activa)
                {
                    _estado.Proyectil.Activa = true;
                    // Forzamos a que nazca un poco más arriba de la punta de la nave
                    _estado.Proyectil.Posicion = new Vector2(_estado.Jugador.Posicion.X, _estado.Jugador.Posicion.Y - 5);
                    _estado.Proyectil.Direccion = new Vector2(0, -8);
                }
            }
        }

        static void ActualizarJuego()
        {
            if (_estado.Proyectil.Activa)
            {
                _estado.Proyectil.Posicion.X += _estado.Proyectil.Direccion.X;
                _estado.Proyectil.Posicion.Y += _estado.Proyectil.Direccion.Y;

                // Si sale de los límites virtuales, apagar la bala
                if (_estado.Proyectil.Posicion.Y < 0 || _estado.Proyectil.Posicion.Y > ALTO_VIRTUAL ||
                    _estado.Proyectil.Posicion.X < 0 || _estado.Proyectil.Posicion.X > ANCHO_VIRTUAL)
                {
                    _estado.Proyectil.Activa = false;
                }
            }
        }

        static void Renderizar()
        {
            // --- 1. PRIMERO BORRAMOS TODOS LOS RASTROS VIEJOS ---
            // Borrar rastro viejo de la bala
            if (_estado.Proyectil.UltimoX > 0 && _estado.Proyectil.UltimoY > 0)
            {
                Console.SetCursorPosition(_estado.Proyectil.UltimoX, _estado.Proyectil.UltimoY);
                Console.Write(" ");
                _estado.Proyectil.UltimoX = -1;
                _estado.Proyectil.UltimoY = -1;
            }

            // Borrar rastro viejo de la nave
            if (_estado.Jugador.UltimoX > 0 && _estado.Jugador.UltimoY > 0)
            {
                Console.SetCursorPosition(_estado.Jugador.UltimoX, _estado.Jugador.UltimoY);
                Console.Write(" ");
            }

            // --- 2. DIBUJAR MARCOS E INTERFAZ ESTATICA ---
            Console.SetCursorPosition(0, 0);
            Console.Write("┌" + new string('─', _estado.Pantalla.AnchoConsola - 2) + "┐");

            Console.SetCursorPosition(2, 0);
            Console.Write($" WASD: Mover | F: Disparar (*) | Ventana Real: {_estado.Pantalla.AnchoConsola}x{_estado.Pantalla.AltoConsola} ");

            Console.SetCursorPosition(0, _estado.Pantalla.AltoConsola - 1);
            Console.Write("└" + new string('─', _estado.Pantalla.AnchoConsola - 2) + "┘");

            // --- 3. DIBUJAR NUEVAS POSICIONES ---

            // Renderizar la Bala Naranja si está activa
            if (_estado.Proyectil.Activa)
            {
                int balaX = 1 + (_estado.Proyectil.Posicion.X * _estado.Pantalla.LimiteMaxX / 100);
                int balaY = 1 + (_estado.Proyectil.Posicion.Y * _estado.Pantalla.LimiteMaxY / 100);

                if (balaX > 0 && balaX < _estado.Pantalla.AnchoConsola - 1 &&
                    balaY > 0 && balaY < _estado.Pantalla.AltoConsola - 1)
                {
                    Console.SetCursorPosition(balaX, balaY);
                    Console.ForegroundColor = ConsoleColor.DarkYellow; // Color naranja nativo
                    Console.Write("*"); // Usamos un asterisco para mayor visibilidad
                    Console.ResetColor();

                    // Guardamos la posición exacta donde se pintó
                    _estado.Proyectil.UltimoX = balaX;
                    _estado.Proyectil.UltimoY = balaY;
                }
            }

            // Renderizar la Nave
            int naveX = 1 + (_estado.Jugador.Posicion.X * _estado.Pantalla.LimiteMaxX / 100);
            int naveY = 1 + (_estado.Jugador.Posicion.Y * _estado.Pantalla.LimiteMaxY / 100);

            if (naveX >= _estado.Pantalla.AnchoConsola - 1) naveX = _estado.Pantalla.AnchoConsola - 2;
            if (naveX < 1) naveX = 1;
            if (naveY >= _estado.Pantalla.AltoConsola - 1) naveY = _estado.Pantalla.AltoConsola - 2;
            if (naveY < 1) naveY = 1;

            Console.SetCursorPosition(naveX, naveY);
            Console.Write("▲");

            // Guardamos la posición exacta donde se pintó
            _estado.Jugador.UltimoX = naveX;
            _estado.Jugador.UltimoY = naveY;
        }
    }
}
