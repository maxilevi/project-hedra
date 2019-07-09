
PERCENTAGE_DISPLAY = 'Percentage'
COLORED_PERCENTAGE_DISPLAY = 'ColoredPercentage'
FLAT_DISPLAY = 'Flat'

def abs_percentage(number):
    return str(abs(int(number * 100.0))) + '%'

def display_percentage(number):
    return ('-' if number < 0 else '') + abs_percentage(number)

def display_colored_percentage(number):
    percentage = ('-' if number < 0 else '+') + abs_percentage(number)
    if number >= 0:
        return '$(BOLD)(GREEN){' + percentage + '}'
    return '$(BOLD)(RED){' + percentage + '}'

def display_flat(number):
    return '{:.2f}'.format(float(number))

DISPLAY_METHODS = {
    PERCENTAGE_DISPLAY: display_percentage,
    COLORED_PERCENTAGE_DISPLAY: display_colored_percentage,
    FLAT_DISPLAY: display_flat
}

def format(value, display_type):
    return DISPLAY_METHODS[display_type](value)

assert DISPLAY_METHODS[FLAT_DISPLAY](1) == '1.00'
assert DISPLAY_METHODS[FLAT_DISPLAY](1.000) == '1.00'
assert DISPLAY_METHODS[FLAT_DISPLAY](1000) == '1000.00'
assert DISPLAY_METHODS[PERCENTAGE_DISPLAY](1) == '100%'
assert DISPLAY_METHODS[PERCENTAGE_DISPLAY](-1) == '-100%'
assert DISPLAY_METHODS[PERCENTAGE_DISPLAY](0) == '0%'
assert DISPLAY_METHODS[PERCENTAGE_DISPLAY](0.5) == '50%'
assert DISPLAY_METHODS[COLORED_PERCENTAGE_DISPLAY](1) == '$(BOLD)(GREEN){+100%}'
assert DISPLAY_METHODS[COLORED_PERCENTAGE_DISPLAY](-1) == '$(BOLD)(RED){-100%}'