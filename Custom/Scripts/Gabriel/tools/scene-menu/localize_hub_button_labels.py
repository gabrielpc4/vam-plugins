#!/usr/bin/env python3
"""
Rebuild hub UIButton two-line texts in pt-BR (tipo na primeira linha, nome na segunda)
and translate fixed instruction/credit headings in ``Default.json``, then mirror to
``MainMenu.json``.
"""

from __future__ import annotations

import argparse
import copy
import json
import shutil
import sys
from pathlib import Path
from typing import Any

_TOOLS_DIR = Path(__file__).resolve().parent
if str(_TOOLS_DIR) not in sys.path:
    sys.path.insert(0, str(_TOOLS_DIR))
from hub_scene_labels_pt_br import build_scene_hub_label_pt_br


def find_va_root(start: Path) -> Path | None:
    for anc in [start.resolve(), *start.resolve().parents]:
        if (anc / "Saves" / "scene").is_dir():
            return anc
    return None


def _get_text_storable(atom: dict[str, Any]) -> dict[str, Any] | None:
    for st in atom.get("storables", []) or []:
        if isinstance(st, dict) and str(st.get("id")) == "Text":
            return st
    return None


def load_scene_trigger_path(atom: dict[str, Any]) -> str | None:
    """First ``sceneFilePath`` inside Trigger.startActions, or None."""
    for st in atom.get("storables", []) or []:
        if not isinstance(st, dict) or str(st.get("id")) != "Trigger":
            continue
        trig = st.get("trigger") or {}
        for act in trig.get("startActions", []) or []:
            if not isinstance(act, dict):
                continue
            if act.get("receiverTargetName") != "LoadScene":
                continue
            fp = act.get("sceneFilePath")
            if fp:
                return str(fp).strip().replace("\\", "/")
            return None
    return None


TEXT_INSTRUCTIONS_PT_BR = (
    "Para voltar a este seletor, abra o menu do VaM, vá em Scene, escolha "
    "Open Scene, role a lista até aparecer MainMenu.json e carregue o "
    "arquivo.\n\n"
    "Se surgirem erros de plugins ou o menu Preferências do usuário abrir, "
    "acesse a guia Security, em permissões de plugin marque sempre permitir "
    "plugins (necessários ao Easy Mate); feche Preferências quando terminar.\n\n"
    "Para abrir ou fechar o menu Role VaM no Oculus Touch use B/Y; no Vive "
    "use o botão Menu.\n\n"
    "Segure o botão Menu para arrastar o painel para onde quiser.\n\n"
    "Nesse menu aparecem atalhos importantes como Select e Grab World "
    "(mover o mundo).\n\n"
    "Toque em qualquer imagem de botão para carregar uma cena.\n\n"
    "Se você é novo no VaM, não sabe usar Possess ou carregar "
    "MainMenu.json comece pelo tutorial Basics & Getting Started.\n\n"
    "Instalar o VaM num SSD ou numa unidade USB 3.1 rápida melhora bem o "
    "tempo de carregamento das cenas.\n\n"
)


def _replace_if_first_line_heading(text: str, old_heading: str, new_heading: str) -> str:
    lines = text.split("\n")
    if not lines:
        return text
    if lines[0].strip() == old_heading.strip():
        lines[0] = new_heading
        return "\n".join(lines)
    return text


def translate_static_ui_text(atom_id: str, text: str) -> str | None:
    if atom_id == "_Text Instructions":
        return TEXT_INSTRUCTIONS_PT_BR
    if atom_id == "_Text C&GTips":
        return (
            "C&G Studio recomenda abrir o menu do VaM e marcar "
            "Freeze Motion/Sound antes de carregar uma cena C&G. Depois "
            "que a cena carregar desmarque Freeze Motion/Sound para manter "
            "áudio e movimento sincronizados.\n\n"
        )
    if atom_id == "_Text Credits Reddit":
        return _replace_if_first_line_heading(
            text,
            "MADE POSSIBLE BY ALL THE FREE REDDIT POSTERS",
            "VIABILIZADO PELA COMUNIDADE QUE POSTA GRATUITAMENTE NO REDDIT",
        )
    if atom_id == "_Text Credits Patreon":
        t = text.replace(
            "SUPPORT THE CREATORS AND GET MORE CONTENT ON PATREON!",
            "APOIE OS CRIADORES E VEJA MAIS CONTEÚDO NO PATREON!",
        )
        lines = []
        for ln in t.split("\n"):
            u = ln.strip().upper()
            if u == "SCENES":
                ln = "CENAS"
            elif u == "MODELS":
                ln = "MODELOS"
            elif u == "CLOTHING AND HAIR":
                ln = "ROUPAS E CABELOS"
            elif u == "ASSET BUNDLES":
                ln = "PACOTES DE RECURSOS"
            lines.append(ln)
        return "\n".join(lines)
    return None


def rewrite_scene_button_labels_default(data: dict[str, Any]) -> int:
    n = 0
    for atom in data.get("atoms", []) or []:
        if not isinstance(atom, dict):
            continue
        if str(atom.get("type")) != "UIButton":
            continue
        path = load_scene_trigger_path(atom)
        if not path:
            continue
        st = _get_text_storable(atom)
        if st is None:
            continue
        old = str(st.get("text", ""))
        bid = str(atom.get("id", ""))
        new = build_scene_hub_label_pt_br("", bid, path, old)
        if new != old:
            st["text"] = new
            n += 1
    return n


def translate_ui_text_atoms(data: dict[str, Any]) -> int:
    n = 0
    for atom in data.get("atoms", []) or []:
        if not isinstance(atom, dict):
            continue
        aid = str(atom.get("id", ""))
        st = _get_text_storable(atom)
        if st is None:
            continue
        old = str(st.get("text", ""))
        new = translate_static_ui_text(aid, old)
        if new is not None and new != old:
            st["text"] = new
            n += 1
    return n


def dump_scene_same_style(data: dict[str, Any], path: Path) -> None:
    with path.open("w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=3)
        f.write("\n")


def main(argv: list[str]) -> int:
    ap = argparse.ArgumentParser(description=__doc__)
    ap.add_argument(
        "--va-root",
        type=Path,
        default=None,
        help="VaM install root (directory containing Saves/scene). "
        "Default: walk up from script path.",
    )
    ap.add_argument(
        "--dry-run",
        action="store_true",
        help="Report counts only; do not write files.",
    )
    ns = ap.parse_args(argv)

    here = Path(__file__).resolve()
    root = ns.va_root or find_va_root(here)
    if root is None:
        raise SystemExit("VaM root not found (needs Saves/scene directory).")

    default_path = root / "Saves" / "scene" / "Default.json"
    main_path = root / "Saves" / "scene" / "MainMenu.json"
    if not default_path.is_file():
        raise SystemExit("Missing %s" % default_path)

    data = json.loads(default_path.read_text(encoding="utf-8"))
    if ns.dry_run:
        data = copy.deepcopy(data)
    n_btn = rewrite_scene_button_labels_default(data)
    n_ui = translate_ui_text_atoms(data)

    print("UIButton labels: %d  UIText panels: %d" % (n_btn, n_ui))
    if ns.dry_run:
        return 0

    bak = default_path.with_suffix(".json.bak-pt-br")
    shutil.copy2(default_path, bak)
    dump_scene_same_style(data, default_path)
    shutil.copyfile(default_path, main_path)
    print("Backup: %s" % bak)
    print("Wrote %s ; copied -> %s" % (default_path, main_path))
    return 0


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
