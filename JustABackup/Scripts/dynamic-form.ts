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

    export function initialize($element: JQuery): void {
        $form = $element;

        console.log("config element", $form.find('script.configuration'));
        configuration = <DynamicFormConfiguration>JSON.parse($form.find('script.configuration').text());
        console.log("config", configuration);

        activeSection = 1;
        sectionCount = 1 + configuration.sectionProperties.length;
        renderSection(activeSection, configuration.fields);
    }

    function renderSection(id: number, fields: DynamicFormField[]) {
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
    }

}