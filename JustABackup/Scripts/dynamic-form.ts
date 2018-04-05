$(() => {
    let $form = $('form.dynamic');
    if ($form.length === 1) {
        DynamicForm.initialize($form);
    }
});

interface DynamicFormConfiguration {

    completeText: string;

    sectionProperties: string[];

    fields: DynamicFormField[];

}

interface DynamicFormField {

    id: string;

    name: string;

    value: any;

    type: string;

    validation: string[];

    dataSource: string;

}

namespace DynamicForm {

    let $form: JQuery;
    let configuration: DynamicFormConfiguration;

    let activeSection: number;
    let sectionCount: number;

    let providers: number[];

    export function initialize($element: JQuery): void {
        $form = $element;
        
        configuration = <DynamicFormConfiguration>JSON.parse($form.find('script.configuration').text());
        
        initializeSectionEvents();
        initializeSubmitHandler();

        activeSection = 1;
        sectionCount = 1 + configuration.sectionProperties.length;
        renderSection(activeSection, configuration.fields);
    }

    function initializeSubmitHandler(): void {

        $form.on("addClass", "input, select", (e, className) => {
            if (className === 'input-validation-error') {
                let $t = $(e.currentTarget);
                let $p = $t.parent();

                if (!$p.hasClass('has-error')) {
                    $p.addClass('has-error');

                    let data = $t.data();
                    for (let propertyName in data) {
                        if (propertyName !== 'val' && propertyName.startsWith('val')) {
                            $p.append(`<span class="has-error">${data[propertyName]}</span>`);
                        }
                    }
                }
            }
        });
        
        $form.on("removeClass", "input, select", (e, className) => {
            if (className === 'input-validation-error') {
                let $p = $(e.currentTarget).parent();

                $p.removeClass('has-error');
                $p.find('.has-error').remove();
            }
        });

        $form.submit(e => {
            if ($form.valid() === false) {
                // dont post when form is not valid
                return false;
            }

            if (activeSection <= sectionCount) {
                activeSection++;
                $.getJSON(`/api/provider/${providers[activeSection - 2]}/fields`, data => {
                    renderSection(activeSection, <DynamicFormField[]>data);
                });
                return false;
            } else {
                return true;
            }
        });
    }

    function initializeSectionEvents(): void {
        if (configuration && configuration.sectionProperties && configuration.sectionProperties.length > 0) {
            configuration.sectionProperties.forEach((val, i) => {
                $form.on('change', `[name="${val}"]`, e => {
                    providers = calculateProviders();
                    sectionCount = providers.length + 1;
                });
            });
        }
    }

    function calculateProviders(): number[] {
        let providers = [];
        configuration.sectionProperties.forEach(val => {
            let value = $form.find(`[name="${val}"]`).val();
            if (Array.isArray(value)) {
                (<any[]>value).forEach(val => {
                    providers.push(val);
                });
            } else if (value) {
                providers.push(value);
            }
        });

        return providers.map(val => parseInt(val));
    }

    function renderSection(id: number, fields: DynamicFormField[]) {
        $form.find("section").attr("hidden", "hidden");

        let $section = $(`<section data-id="${id}"></section>`);

        fields.forEach(field => {
            let $field = InputTemplates.generateInput(field, activeSection);
            $section.append($field);
        });

        let submitText = "Continue";
        if (id == sectionCount) {
            submitText = configuration.completeText;
        }

        $section.append(`<div class="form-group text-right"><input type="submit" value="${submitText}" class="btn btn-primary"></div>`);
        $section.appendTo($form);

        cron.activateCron($section);
        $.validator.unobtrusive.parse($form);
    }

}