using CyberdropEZDownloaderGraphical.ListBase;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CyberdropEZDownloaderGraphical
{
    public interface IProgression
    {
        Task<(List<BaseCyberdrop>, bool)> TelechargerContenu(List<BaseCyberdrop> liste_docs);
    }
}
