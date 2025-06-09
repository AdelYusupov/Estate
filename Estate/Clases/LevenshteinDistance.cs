using System;

public class LevenshteinDistance
{
    /// <summary>
    /// Вычисляет расстояние Левенштейна между двумя строками
    /// </summary>
    /// <param name="s1">Первая строка</param>
    /// <param name="s2">Вторая строка</param>
    /// <returns>Расстояние Левенштейна</returns>
    public static int Calculate(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1))
        {
            return string.IsNullOrEmpty(s2) ? 0 : s2.Length;
        }

        if (string.IsNullOrEmpty(s2))
        {
            return s1.Length;
        }

        int[,] matrix = new int[s1.Length + 1, s2.Length + 1];

        // Инициализация первого столбца и первой строки
        for (int i = 0; i <= s1.Length; i++)
        {
            matrix[i, 0] = i;
        }

        for (int j = 0; j <= s2.Length; j++)
        {
            matrix[0, j] = j;
        }

        // Заполнение матрицы
        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;

                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1,     // Удаление
                    matrix[i, j - 1] + 1),             // Вставка
                    matrix[i - 1, j - 1] + cost);      // Замена
            }
        }

        return matrix[s1.Length, s2.Length];
    }

    /// <summary>
    /// Проверяет, соответствует ли строка target критерию поиска по расстоянию Левенштейна
    /// </summary>
    /// <param name="source">Исходная строка для сравнения</param>
    /// <param name="target">Целевая строка для проверки</param>
    /// <param name="maxDistance">Максимально допустимое расстояние</param>
    /// <returns>True, если расстояние между строками <= maxDistance</returns>
    public static bool IsMatch(string source, string target, int maxDistance)
    {
        return Calculate(source, target) <= maxDistance;
    }
}