#!/usr/bin/env python3
"""
Brazilian Portuguese labels for VaM Easy Mate hub UIButton scene slots.

Used by ``rewire_hub_scene_button.py`` and ``localize_hub_button_labels.py``.

Format:
  Linha 1: tipo em pt-BR
  Linha 2: nome exibido (título já usado quando possível)
"""

from __future__ import annotations

import re
from pathlib import PurePosixPath

# Exact English hub ``Type:`` phrases (stripped lower-case key).
_TYPES_EN_LOWER_PT: dict[str, str] = {
    "story": "História",
    "dance": "Dança",
    "dance &": "Dança e …",
    "tutorial": "Tutorial",
    "configurable scene": "Cena configurável",
    "dance near me": "Dança perto de mim",
    "kissing and touching": "Beijos e carícias",
    "lap dance": "Dança no colo",
    "missionary scene": "Missionário",
    "riding scene": "Cavalgada",
    "person": "Pessoa",
    "positions menu": "Menu de posições",
}

_CONDENSED_PRE_TYPE_PT: dict[str, str] = {
    "SIMULATING SEX": "Simulação sexual",
    "EASY MIX & SCENE BUILDING": "Easy Mix e construção de cenas",
    "BASICS & GETTING STARTED": "Básicos e primeiros passos",
    "PLAYING WITH PEOPLE & NEW EASY BUTTONS": (
        "Jogando com pessoas e novos botões fáceis"
    ),
}

_PRIMARY_PT_TYPE_LINES = frozenset(
    {*_TYPES_EN_LOWER_PT.values(), "Tutorial", "Movimento manual"}
    | frozenset(
        {
            "História",
            "Dança",
            "Cenas",
            "Cena",
            "Menu de aparências",
            "Mais páginas",
            "Menu de posições",
        }
    )
)


def _norm_scene_path_for_sniff(scene_path: str) -> str:
    return scene_path.strip().replace("\\", "/")


def extract_type_en_from_full_text(old_full_text: str) -> str | None:
    m = re.search(r"(?mi)^Type:\s*(.+)\s*$", old_full_text)
    if not m:
        return None
    s = m.group(1).strip()
    return s if s else None


def translate_type_en_to_pt(type_en: str) -> str | None:
    k = type_en.strip().lower()
    if k in _TYPES_EN_LOWER_PT:
        return _TYPES_EN_LOWER_PT[k]
    if k.startswith("manual movement"):
        return "Movimento manual"
    return None


def path_sniff_type_pt(scene_path: str) -> str | None:
    s = _norm_scene_path_for_sniff(scene_path)
    if "PersonLooksMenu" in s:
        return "Menu de aparências"
    if "MainMenu_Page" in s:
        return "Mais páginas"
    if "VAMasutraMenu" in s or "vamasutra" in s.lower():
        return "Menu de posições"
    return None


def default_column_type_pt(button_id: str) -> str | None:
    if button_id.startswith("Left"):
        return "Dança"
    if button_id.startswith("Front"):
        return "História"
    if button_id.startswith("Right"):
        return "Cenas"
    return None


def resolve_type_pt(button_id: str, scene_path: str, old_full_text: str) -> str:
    te = extract_type_en_from_full_text(old_full_text)
    if te:
        hit = translate_type_en_to_pt(te)
        if hit:
            return hit
    sniff = path_sniff_type_pt(scene_path)
    if sniff:
        return sniff
    col = default_column_type_pt(button_id)
    if col:
        return col
    return "Cena"


def extract_body_before_hub_meta(raw: str) -> str:
    """
    Lines before hub ``Type:`` / ``Author:`` markers (drops tutorial footers).
    """
    lines_out: list[str] = []
    for ln in raw.splitlines():
        ls = ln.strip()
        if re.match(r"(?i)^type:\s*", ls):
            break
        if re.match(r"(?i)^author:\s*", ls):
            break
        lines_out.append(ln)
    return "\n".join(lines_out).rstrip()


def first_non_empty_line(s: str) -> str:
    for ln in s.splitlines():
        if ln.strip():
            return ln.strip()
    return ""


def condensed_pre_type_title(body: str) -> str:
    parts = [p.strip() for p in body.splitlines() if p.strip()]
    cu = " ".join(parts).upper()
    cu = " ".join(cu.split())
    if cu in _CONDENSED_PRE_TYPE_PT:
        return _CONDENSED_PRE_TYPE_PT[cu]
    return " ".join(parts)


def _is_nav_chrome_title(line: str) -> bool:
    u = line.strip().upper()
    if u == "LOOKS MENU":
        return True
    if u.startswith("MENU PAGE"):
        return True
    if u == "VAMASUTRA MENU":
        return True
    return False


def _page_num_from_looks_block(trimmed: str) -> int | None:
    m = re.search(r"(?mi)^PAGE\s+(\d+)\s*$", trimmed, re.MULTILINE)
    if m:
        return int(m.group(1))
    return None


def _page_num_from_menu_page_line(line: str) -> int | None:
    m = re.match(r"(?i)^MENU\s+PAGE\s+(\d+)\s*$", line.strip())
    if m:
        return int(m.group(1))
    return None


def title_from_scene_path_stem(scene_path: str) -> str:
    s = _norm_scene_path_for_sniff(scene_path)
    if not s:
        return ""
    name = PurePosixPath(s).name
    stem = name.rsplit(".", 1)[0] if "." in name else name
    return stem.replace("_", " ").strip()


def polish_title_line(title: str, scene_path: str) -> str:
    """
    Light PT pass for menu chrome; scene names mostly stay unchanged.
    """
    t = title.strip()
    if not t:
        t = title_from_scene_path_stem(scene_path)
    m = _page_num_from_menu_page_line(t)
    if m is not None:
        return "Página %d" % (m,)
    u = " ".join(t.upper().split())
    repl = {
        "SIMULATING": "Simulando",
        "EASY MIX": "Easy Mix",
        "PLAYING": "Jogando",
        "BASICS &": "Básico e",
    }
    if u in repl:
        return repl[u]
    return t


def split_prior_pt_two_line_hub_label(
    old_full_text: str, scene_path: str
) -> tuple[str, str] | None:
    """
    If text is already migrated ``tipo`` + ``título`` (two lines), keep both lines.
    Ignores legacy English blocks that still have ``Type:`` / ``Author:``.
    """
    if extract_type_en_from_full_text(old_full_text):
        return None
    if re.search(r"(?mi)^author:\s", old_full_text):
        return None
    parts = old_full_text.strip().split("\n")
    if len(parts) != 2:
        return None
    a = parts[0].strip()
    b = parts[1].strip()
    if not a or not b:
        return None
    if a not in _PRIMARY_PT_TYPE_LINES:
        return None
    hub = _norm_scene_path_for_sniff(scene_path)
    return a, polish_title_line(b, hub)


def derive_scene_button_title_pt(
    old_full_text: str, scene_path: str, button_id: str
) -> str:
    sfp = _norm_scene_path_for_sniff(scene_path)
    trimmed = extract_body_before_hub_meta(old_full_text)
    condensed = condensed_pre_type_title(trimmed)
    first = first_non_empty_line(trimmed)

    mpm = re.search(r"(?i)MainMenu_Page_(\d+)\.json", sfp)
    if mpm:
        try:
            return "Página %d" % (int(mpm.group(1)),)
        except ValueError:
            pass

    if "PersonLooksMenu" in sfp:
        n = _page_num_from_looks_block(trimmed)
        if n is not None:
            return "Página %d" % (n,)
        return "Menu de aparências"

    tun = condensed.upper()
    tun = " ".join(tun.split())
    if tun in _CONDENSED_PRE_TYPE_PT:
        return _CONDENSED_PRE_TYPE_PT[tun]

    te = extract_type_en_from_full_text(old_full_text)
    if te and te.strip().lower() == "tutorial" and condensed.strip():
        return condensed.strip()

    if "VAMasutraMenu" in sfp:
        if first:
            fu = first.upper().strip()
            if "VAMASUTRA" in fu:
                return "Menu VAMasutra"
            return polish_title_line(first, scene_path)
        return "Menu VAMasutra"

    if first and _is_nav_chrome_title(first):
        return polish_title_line(title_from_scene_path_stem(scene_path), scene_path)

    if first:
        return polish_title_line(first, scene_path)

    return polish_title_line(title_from_scene_path_stem(sfp), sfp)


def format_two_line_pt(type_pt: str, title_pt: str) -> str:
    return "%s\n%s" % (type_pt.strip(), title_pt.strip())


def build_scene_hub_label_pt(
    scene_title_stem: str,
    button_id: str,
    scene_hub_path: str,
    old_full_text: str | None,
) -> str:
    """Two-line PT label for UIButton scenes (existing text optional)."""
    hub = scene_hub_path.strip().replace("\\", "/")
    if old_full_text is not None:
        frozen = split_prior_pt_two_line_hub_label(old_full_text, hub)
        if frozen is not None:
            return format_two_line_pt(frozen[0], frozen[1])
        title = derive_scene_button_title_pt(old_full_text, hub, button_id)
    else:
        title = polish_title_line(scene_title_stem.strip(), hub)
    typ = resolve_type_pt(button_id, hub, old_full_text or "")
    return format_two_line_pt(typ, title)
