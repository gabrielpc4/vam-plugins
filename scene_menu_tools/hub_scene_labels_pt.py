#!/usr/bin/env python3
"""
Brazilian Portuguese labels for VaM Easy Mate hub UIButton scene slots.

Used by ``rewire_hub_scene_button.py`` and ``localize_hub_button_labels.py``.

Format:
  Linha 1: ``Tipo:`` em pt-BR
  Linha 2: nome da cena (traduzido, com exceções)
  ``\\n`` iniciais puxam o bloco para baixo porque o VaM centraliza todo o texto
  no espaço do botão.

Exceções: tipo ``Dança`` preserva inglês (``Like a Dark Horse`` etc.); duas músicas,
``Hey Mama`` e ``Late Nite Dance``, recebem só toque leve PT; título com ``Dawn``
permanece (ex.: Crack of Dawn).
``1100_camgirltoys`` → ``Camgirl and Toys``.
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

_CAMGIRL_STEM_LOWER = "1100_camgirltoys"
_CAMGIRL_DISPLAY_TITLE = "Camgirl and Toys"

_SCENE_TITLE_NORMALIZED_PT: dict[str, str] = {
    _CAMGIRL_STEM_LOWER: _CAMGIRL_DISPLAY_TITLE,
    "like a dark horse": "Como um cavalo negro",
    "best laid plans": "Os melhores planos traçados",
    "seductive witches": "Bruxas sedutoras",
    "jessika's bedroom": "Quarto da Jessika",
    "rainy mood missionary": "Missionário em clima chuvoso",
    "late nite dance": "Dança da madrugada",
    "yolanda x rose jerk-off and bj (futa)": (
        "Yolanda x Rose punheta e boquete (Futa)"
    ),
    "blue": "Azul",
    "makeout session": "Sessão de amassos",
    "a dressing room": "Sala de vestir",
    "hacker cracker": "Hacker cracker",
    "touchy booty shake": "Rebolado sensível",
    "not so rude awakening": "Um despertar nada rude",
    "no matter": "Não importa",
    "worthy": "Valor",
    "call me": "Me liga",
    "yolanda - bbc mod": "Yolanda — mod BBC",
    "bondage and pole dance": "Bondagem e pole dance",
    "math lesson": "Lição de matemática",
    "spanking, press next for reverse cowgirl spanking": (
        "Surras: avance pra cowgirl invertida"
    ),
}

# Leading ``\\n``: VaM centraliza o bloco inteiro; uns poucos descem o rótulo um
# pouco. Demais (ex.: 14) empurra o texto para fora do quadro — ajuste fino aqui.
_HUB_UIBUTTON_LEADING_PAD_NEWLINES = 6


def _norm_scene_path_for_sniff(scene_path: str) -> str:
    return scene_path.strip().replace("\\", "/")


def _normalize_title_lookup_key(s: str) -> str:
    return " ".join(s.strip().split()).lower()


def strip_tipo_colon_suffix(line: str) -> str:
    s = line.strip()
    while s.endswith(":"):
        s = s[:-1].rstrip()
    return s.strip()


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


_DANCA_STILL_TRANSLATE_TITLE: dict[str, str] = {
    _normalize_title_lookup_key(k): v
    for k, v in {
        "Hey Mama - Trap Mix": "Hey Mama — mix trap",
        "Late Nite Dance": "Dança da madrugada",
    }.items()
}


def polish_title_line(title: str, scene_path: str) -> str:
    t = title.strip()
    if not t:
        t = title_from_scene_path_stem(scene_path)
    lp = (_norm_scene_path_for_sniff(scene_path)).lower()
    fn = PurePosixPath(lp).name.lower()
    if _CAMGIRL_STEM_LOWER in lp or fn.startswith(_CAMGIRL_STEM_LOWER):
        return _CAMGIRL_DISPLAY_TITLE
    pg = _page_num_from_menu_page_line(t)
    if pg is not None:
        return "Página %d" % (pg,)
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


def translate_scene_display_title(
    title_pt_or_en: str, type_pt: str, scene_path: str
) -> str:
    sfp_l = (_norm_scene_path_for_sniff(scene_path)).lower()
    fn = PurePosixPath(sfp_l).name.lower()
    if _CAMGIRL_STEM_LOWER in sfp_l or fn.startswith(_CAMGIRL_STEM_LOWER):
        return _CAMGIRL_DISPLAY_TITLE

    tit = title_pt_or_en.strip()
    if not tit:
        return tit

    if type_pt.strip() == "Dança":
        lk_d = _normalize_title_lookup_key(tit)
        if lk_d in _DANCA_STILL_TRANSLATE_TITLE:
            return _DANCA_STILL_TRANSLATE_TITLE[lk_d]
        return tit
    if re.search(r"(?<![A-Za-z0-9])Dawn(?![A-Za-z0-9])", tit):
        return tit

    tl = tit.lower().strip()
    if tl.startswith("página ") or tl.startswith("pagina "):
        return tit
    if tit in ("Menu de aparências", "Menu VAMasutra"):
        return tit

    lk = _normalize_title_lookup_key(tit)
    if lk in _SCENE_TITLE_NORMALIZED_PT:
        return _SCENE_TITLE_NORMALIZED_PT[lk]
    stm = lk.replace(".json", "").replace(".vac", "")
    if stm in _SCENE_TITLE_NORMALIZED_PT:
        return _SCENE_TITLE_NORMALIZED_PT[stm]
    return tit


def split_prior_pt_two_line_hub_label(
    old_full_text: str, scene_path: str
) -> tuple[str, str] | None:
    if extract_type_en_from_full_text(old_full_text):
        return None
    if re.search(r"(?mi)^author:\s", old_full_text):
        return None
    lines = old_full_text.split("\n")
    i = 0
    while i < len(lines) and not lines[i].strip():
        i += 1
    if i >= len(lines):
        return None
    tipo_key = strip_tipo_colon_suffix(lines[i].strip())
    i += 1
    if tipo_key not in _PRIMARY_PT_TYPE_LINES:
        return None
    while i < len(lines) and not lines[i].strip():
        i += 1
    if i >= len(lines):
        return None
    b_raw = lines[i].strip()
    i += 1
    while i < len(lines):
        if lines[i].strip():
            return None
        i += 1
    hub = _norm_scene_path_for_sniff(scene_path)
    return tipo_key, polish_title_line(b_raw, hub)


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
    a = strip_tipo_colon_suffix(type_pt.strip())
    b = title_pt.strip()
    lead = "\n" * _HUB_UIBUTTON_LEADING_PAD_NEWLINES
    return "%s%s:\n%s" % (lead, a, b)


def build_scene_hub_label_pt(
    scene_title_stem: str,
    button_id: str,
    scene_hub_path: str,
    old_full_text: str | None,
) -> str:
    hub = scene_hub_path.strip().replace("\\", "/")
    typ = resolve_type_pt(button_id, hub, old_full_text or "")
    if old_full_text is not None:
        frozen = split_prior_pt_two_line_hub_label(old_full_text, hub)
        if frozen is not None:
            title_base = frozen[1]
        else:
            title_base = derive_scene_button_title_pt(
                old_full_text, hub, button_id
            )
    else:
        title_base = polish_title_line(scene_title_stem.strip(), hub)
    titled = translate_scene_display_title(title_base, typ, hub)
    return format_two_line_pt(typ, titled)
