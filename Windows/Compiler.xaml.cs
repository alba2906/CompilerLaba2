using Laba1.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Laba1.Windows
{
    public partial class Compiler : Window
    {
        private bool _isFileModified = false;
        private string _currentFilePath = string.Empty;
        private readonly LexicalAnalyzer _analyzer = new();

        public Compiler()
        {
            InitializeComponent();
            UpdateWindowTitle();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (_isFileModified)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Текущий файл был изменен.\nСохранить изменения?",
                    "Подтверждение",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                    Save_Click(sender, e);
                else if (result == MessageBoxResult.Cancel)
                    return;
            }

            FileContentViewer.Document.Blocks.Clear();
            FileContentViewer.Document.Blocks.Add(new Paragraph(new Run(string.Empty)));

            OutputDataGrid.ItemsSource = null;

            _isFileModified = false;
            _currentFilePath = string.Empty;
            UpdateWindowTitle();
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Poo файлы (*.poo)|*.poo|Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    TextRange textRange = new(
                        FileContentViewer.Document.ContentStart,
                        FileContentViewer.Document.ContentEnd);

                    using FileStream fs = new(openFileDialog.FileName, FileMode.Open);
                    textRange.Load(fs, DataFormats.Text);

                    _currentFilePath = openFileDialog.FileName;
                    _isFileModified = false;
                    OutputDataGrid.ItemsSource = null;
                    UpdateWindowTitle();

                    MessageBox.Show(
                        $"Файл успешно загружен: {Path.GetFileName(_currentFilePath)}",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Ошибка при загрузке файла: {ex.Message}",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath))
                SaveAs_Click(sender, e);
            else
                SaveToFile(_currentFilePath);
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new()
            {
                Filter = "Poo файлы (*.poo)|*.poo|Текстовые файлы (*.txt)|*.txt",
                DefaultExt = "poo",
                AddExtension = true,
                FilterIndex = 1
            };

            if (!string.IsNullOrEmpty(_currentFilePath))
            {
                saveFileDialog.InitialDirectory = Path.GetDirectoryName(_currentFilePath);
                saveFileDialog.FileName = Path.GetFileName(_currentFilePath);
            }
            else
            {
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                saveFileDialog.FileName = "NewFile.poo";
            }

            if (saveFileDialog.ShowDialog() == true)
            {
                _currentFilePath = saveFileDialog.FileName;
                SaveToFile(_currentFilePath);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (_isFileModified)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Есть несохраненные изменения.\nВы действительно хотите выйти?",
                    "Подтверждение выхода",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                    Application.Current.Shutdown();
            }
            else
            {
                MessageBoxResult result = MessageBox.Show(
                    "Вы действительно хотите выйти из программы?",
                    "Подтверждение выхода",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                    Application.Current.Shutdown();
            }
        }

        private void Reference_Click(object sender, RoutedEventArgs e)
        {
            Reference reference = new();
            reference.Show();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new();
            aboutWindow.Show();
        }

        private void SaveToFile(string filePath)
        {
            try
            {
                TextRange textRange = new(
                    FileContentViewer.Document.ContentStart,
                    FileContentViewer.Document.ContentEnd);

                if (string.IsNullOrWhiteSpace(textRange.Text))
                {
                    MessageBoxResult result = MessageBox.Show(
                        "Файл пуст.\nВсе равно сохранить?",
                        "Подтверждение",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.No)
                        return;
                }

                using FileStream fs = new(filePath, FileMode.Create);
                textRange.Save(fs, DataFormats.Text);

                _isFileModified = false;
                UpdateWindowTitle();

                MessageBox.Show(
                    $"Файл успешно сохранен:\n{filePath}",
                    "Сохранение завершено",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Ошибка при сохранении файла:\n{ex.Message}",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void FileContentViewer_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextRange textRange = new(
                FileContentViewer.Document.ContentStart,
                FileContentViewer.Document.ContentEnd);

            string text = textRange.Text.Trim();
            bool hasContent = !string.IsNullOrWhiteSpace(text);

            _isFileModified = hasContent;
            UpdateWindowTitle();
        }

        private void UpdateWindowTitle()
        {
            string fileName = string.IsNullOrEmpty(_currentFilePath)
                ? "Новый файл"
                : Path.GetFileName(_currentFilePath);

            string modifiedMarker = _isFileModified ? "*" : "";
            Title = $"Компилятор: {fileName}{modifiedMarker}";
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (_isFileModified)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Есть несохраненные изменения.\nСохранить перед выходом?",
                    "Подтверждение",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                    Save_Click(this, new RoutedEventArgs());
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            base.OnClosing(e);
        }

        private void RunAnalysis_Click(object sender, RoutedEventArgs e)
        {
            TextRange textRange = new(
                FileContentViewer.Document.ContentStart,
                FileContentViewer.Document.ContentEnd);

            string sourceText = textRange.Text;

            // Очистка старых результатов перед новым запуском
            OutputDataGrid.ItemsSource = null;

            AnalysisResult analysisResult = _analyzer.Analyze(sourceText);
            List<ResultRow> rows = new();

            foreach (var token in analysisResult.Tokens)
            {
                rows.Add(new ResultRow
                {
                    Code = token.Code.ToString(),
                    TypeName = GetTokenTypeName(token.Type),
                    Lexeme = token.Lexeme,
                    Location = token.Location,
                    Line = token.Line,
                    Column = token.StartColumn,
                    IsError = false
                });
            }

            foreach (var error in analysisResult.Errors)
            {
                rows.Add(new ResultRow
                {
                    Code = "99",
                    TypeName = "лексическая ошибка",
                    Lexeme = string.IsNullOrWhiteSpace(error.LexemeRepresentation)
                        ? error.Message
                        : error.LexemeRepresentation,
                    Location = error.Location,
                    Line = error.Line,
                    Column = error.Column,
                    IsError = true
                });
            }

            OutputDataGrid.ItemsSource = rows;
        }

        private void OutputDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OutputDataGrid.SelectedItem is not ResultRow row)
                return;

            TextPointer? pointer = TextPositionHelper.GetTextPointerAt(
                FileContentViewer,
                row.Line,
                row.Column);

            if (pointer == null)
                return;

            FileContentViewer.Focus();
            FileContentViewer.CaretPosition = pointer;
            FileContentViewer.Selection.Select(pointer, pointer.GetPositionAtOffset(1) ?? pointer);
        }

        private static string GetTokenTypeName(TokenType type)
        {
            return type switch
            {
                TokenType.UnsignedInteger => "целое без знака",
                TokenType.Identifier => "идентификатор",
                TokenType.KeywordNew => "ключевое слово new",
                TokenType.KeywordString => "ключевое слово типа string",
                TokenType.KeywordInt => "ключевое слово типа int",
                TokenType.StringLiteral => "строковый литерал",
                TokenType.LessThan => "символ <",
                TokenType.GreaterThan => "символ >",
                TokenType.Comma => "символ ,",
                TokenType.Assign => "оператор присваивания",
                TokenType.Whitespace => "разделитель (пробел)",
                TokenType.OpenBrace => "символ {",
                TokenType.CloseBrace => "символ }",
                TokenType.Semicolon => "конец оператора",
                _ => "неизвестно"
            };
        }

    }

    public sealed class ResultRow
    {
        public string Code { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public string Lexeme { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int Line { get; set; }
        public int Column { get; set; }
        public bool IsError { get; set; }
    }
}