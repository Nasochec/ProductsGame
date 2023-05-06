﻿using ProductionsGameCore;
using ProductsGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ProductsGameLauncher
{
    /// <summary>
    /// Interaction logic for GameResults.xaml
    /// </summary>
    public partial class GameResults : Window
    {
        private GameCompiler compiler;
        public GameResults()
        {
            InitializeComponent();
        }

        public GameResults(GameCompiler gc)
        {
            InitializeComponent();
            this.compiler = gc;
            compiler.play();
        }

        public GameResults(GameSettings gs,List<string> tournamentPlayersFilenames)
        {
            InitializeComponent();
            this.compiler = gc;
            compiler.play();
        }

    }
}
